using Hospital.Shared.Proxies;
using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.Web.Models;
using Hospital.Web.Models.Admin;
using Hospital.Web.Models.Patients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using Hospital.Data.Models;
using Hospital.Data.Models;
using Hospital.Data.Models;
using Hospital.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hospital.Web.Controllers;

[Authorize]
public class AdminController : Controller
{
    private const string BatchDateFormat = "yyyy-MM-dd";
    private const string SubstanceLineDelimiter = ":";
    private static readonly string[] LineDelimiters = { "\r\n", "\n" };

    private readonly IAdminApiClient adminService;
    private readonly IPatientApiClient patientService;
    private readonly IAllergyApiClient allergyService;

    public AdminController(IAdminApiClient adminService, IPatientApiClient patientService, IAllergyApiClient allergyService)
    {
        this.adminService = adminService;
        this.patientService = patientService;
        this.allergyService = allergyService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchQuery, int? minAge, int? maxAge, Sex? sex, bool archived = false, int? selectedId = null, CancellationToken cancellationToken = default)
    {
        var searchResults = await SearchPatientsAsync(searchQuery, minAge, maxAge, sex, cancellationToken);
        var visiblePatients = searchResults.Where(patient => patient.IsArchived == archived).OrderBy(patient => patient.LastName).ToList();
        Patient? selectedPatient = selectedId.HasValue ? (visiblePatients.FirstOrDefault(patient => patient.PatientId == selectedId.Value) ?? await patientService.GetByIdAsync(selectedId.Value, cancellationToken)) : null;

        return View(new AdminPatientsIndexViewModel
        {
            Patients = visiblePatients.Select(MapPatientListItem).ToList(),
            SelectedPatient = selectedPatient != null ? MapEditPatient(selectedPatient, null) : null,
            SearchQuery = searchQuery,
            ShowArchived = archived
        });
    }

    [HttpGet]
    public async Task<IActionResult> Items(string searchQuery = "", bool showExpiredOnly = false, CancellationToken cancellationToken = default)
    {
        var searchResults = await SearchPatientsAsync(searchQuery, minAge, maxAge, sex, cancellationToken);
        var visiblePatients = searchResults.Where(p => p.IsArchived == archived).OrderBy(p => p.LastName).ToList();
        Patient? selectedPatient = selectedId.HasValue ? (visiblePatients.FirstOrDefault(p => p.PatientId == selectedId.Value) ?? await patientService.GetByIdAsync(selectedId.Value, cancellationToken)) : null;

        return View(new AdminPatientsIndexViewModel
        {
            Patients = visiblePatients.Select(MapPatientListItem).ToList(),
            SelectedPatient = selectedPatient != null ? MapEditPatient(selectedPatient, null) : null,
            SelectedPatientId = selectedId,
            SearchQuery = searchQuery,
            MinAge = minAge,
            MaxAge = maxAge,
            Sex = sex,
            ShowArchived = archived
        });
    }

    [HttpGet]
    public async Task<IActionResult> ItemDetails(int id, CancellationToken cancellationToken = default)
    {
        Item? item = await this.adminService.GetItemByIdAsync(id, cancellationToken);
        return item == null ? NotFound() : View(item);
    }

    [HttpGet]
    public IActionResult Create() => View(new ItemViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ItemViewModel viewModel, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return View(viewModel);
        await this.adminService.CreateItemWithQuantityAsync(viewModel.Name, viewModel.Producer, viewModel.Category, viewModel.Price, viewModel.NumberOfPills, viewModel.Quantity, ParseSubstancesText(viewModel.SubstancesText), ParseBatchesText(viewModel.BatchesText), viewModel.Label, viewModel.Description, viewModel.ImagePath ?? string.Empty, viewModel.DiscountPercentage, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        Item? item = await this.adminService.GetItemByIdAsync(id, cancellationToken);
        return item == null ? NotFound() : View(MapItemToViewModel(item));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ItemViewModel viewModel, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return View(viewModel);
        var updatedItem = new Item(id, viewModel.Name, viewModel.Producer, viewModel.Category, viewModel.Price, viewModel.NumberOfPills, viewModel.Label, viewModel.Description, viewModel.ImagePath ?? string.Empty, viewModel.DiscountPercentage, viewModel.Quantity)
        { ActiveSubstances = ParseSubstancesText(viewModel.SubstancesText), Batches = ParseBatchesText(viewModel.BatchesText) };
        await this.adminService.UpdateItemAsync(updatedItem, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken = default)
    {
        await this.adminService.DeleteItemAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Patients(string? searchQuery, int? minAge, int? maxAge, Sex? sex, bool archived = false, int? selectedId = null, CancellationToken cancellationToken = default)
    {
        var searchResults = await SearchPatientsAsync(searchQuery, minAge, maxAge, sex, cancellationToken);
        var visiblePatients = searchResults.Where(p => p.IsArchived == archived).OrderBy(p => p.LastName).ToList();
        Patient? selectedPatient = selectedId.HasValue ? (visiblePatients.FirstOrDefault(p => p.PatientId == selectedId.Value) ?? await patientService.GetByIdAsync(selectedId.Value, cancellationToken)) : null;

        return View("Index", new AdminPatientsIndexViewModel
        {
            Patients = visiblePatients.Select(MapPatientListItem).ToList(),
            SelectedPatient = selectedPatient != null ? MapEditPatient(selectedPatient, null) : null,
            SearchQuery = searchQuery,
            ShowArchived = archived
        });
    }

    private static ItemViewModel MapItemToViewModel(Item item) => new() 
    { 
        Id = item.Id, 
        Name = item.Name, 
        Producer = item.Producer, 
        Price = item.Price, 
        Category = item.Category, 
        ImagePath = item.ImagePath, 
        NumberOfPills = item.NumberOfPills, 
        Quantity = item.Quantity, 
        Label = item.Label, 
        Description = item.Description, 
        DiscountPercentage = item.DiscountPercentage, 
        SubstancesText = string.Join(Environment.NewLine, item.ActiveSubstances.Select(env => $"{env.Key}:{env.Value}")), 
        BatchesText = string.Join(Environment.NewLine, item.Batches.Select(env => $"{env.Key:yyyy-MM-dd}:{env.Value}")) 
    };
    private static Dictionary<string, float> ParseSubstancesText(string text) => string.IsNullOrWhiteSpace(text) ? new() : text.Split(LineDelimiters, StringSplitOptions.RemoveEmptyEntries).Select(line => line.Split(':')).ToDictionary(patient => patient[0].Trim(), patient => float.Parse(patient[1]));
    private static Dictionary<DateOnly, int> ParseBatchesText(string text) => string.IsNullOrWhiteSpace(text) ? new() : text.Split(LineDelimiters, StringSplitOptions.RemoveEmptyEntries).Select(line => line.Split(':')).ToDictionary(patient => DateOnly.Parse(patient[0]), patient => int.Parse(patient[1]));
    private static PatientListItemViewModel MapPatientListItem(Patient patient) => new() { Id = patient.PatientId, FirstName = patient.FirstName, LastName = patient.LastName, Cnp = patient.Cnp, Dob = patient.DateOfBirth, Sex = patient.Sex.ToString(), PhoneNo = FormatPhoneNumber(patient.PhoneNumber), EmergencyContact = FormatPhoneNumber(patient.EmergencyContact), IsArchived = patient.IsArchived, IsDeceased = patient.DateOfDeath.HasValue };
    private static EditPatientViewModel MapEditPatient(Patient patient, PatientListItemViewModel? _) => new() { Id = patient.PatientId, FirstName = patient.FirstName, LastName = patient.LastName, Cnp = patient.Cnp, Dob = patient.DateOfBirth, Dod = patient.DateOfDeath, Sex = patient.Sex, PhoneNo = CompactPhoneNumber(patient.PhoneNumber), EmergencyContact = CompactEmergencyContact(patient.EmergencyContact), IsArchived = patient.IsArchived, IsDonor = patient.IsDonor, Transferred = patient.Transferred };
    private static string NormalizePhone(string phone) => string.IsNullOrWhiteSpace(phone) ? phone : phone.Replace(" ", "").Replace("-", "");
    private static string FormatPhoneNumber(string phone) => string.IsNullOrWhiteSpace(phone) || phone.Length != 10 ? phone : $"+40 {phone.Substring(1, 3)} {phone.Substring(4, 3)} {phone.Substring(7, 3)}";
    private static string CompactPhoneNumber(string phone) => string.IsNullOrWhiteSpace(phone) ? phone : (phone.StartsWith("0") && phone.Length == 10 ? $"+40{phone[1..]}" : phone);
    private static string CompactEmergencyContact(string contact) => string.IsNullOrWhiteSpace(contact) ? contact : string.Join(",", contact.Split(',').Select(patient => patient.Trim()).Select(patient => patient.Any(char.IsDigit) ? CompactPhoneNumber(patient) : patient));
    private static List<string> SplitConditions(string? text) => string.IsNullOrWhiteSpace(text) ? new() : text.Split(',').Select(condition => condition.Trim()).Where(condition => !string.IsNullOrWhiteSpace(condition)).ToList();
    private async Task<List<Patient>> SearchPatientsAsync(string? queue, int? min, int? max, Sex? sex, CancellationToken cancel) => await patientService.SearchPatientsAsync(new SearchPatientsRequest { MinAge = min, MaxAge = max, Sex = sex, Cnp = (queue?.All(char.IsDigit) == true && queue.Length == 13) ? queue : null, NamePart = (queue?.All(char.IsDigit) == false) ? queue : null }, cancel);
    private IActionResult RedirectToLogin() => RedirectToAction("Login", "Auth");
}
