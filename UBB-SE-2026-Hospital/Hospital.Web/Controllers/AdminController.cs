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
using Hospital.Data.Models.DTOs;
using Hospital.Shared.Models;
using Hospital.Shared.DTOs;
using Hospital.Services.PatientEr;
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

    private readonly IAdminService adminService;
    private readonly IPatientService patientService;
    private readonly IAllergyService allergyService;

    public AdminController(IAdminService adminService, IPatientService patientService, IAllergyService allergyService)
    {
        this.adminService = adminService;
        this.patientService = patientService;
        this.allergyService = allergyService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string searchQuery = "", bool showExpiredOnly = false, CancellationToken cancellationToken = default)
    {
        var items = await this.adminService.GetItemsAsync(searchQuery, cancellationToken);
        if (showExpiredOnly)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            items = items.Where(i => i.Batches.Keys.Any(expiryDate => expiryDate < today)).ToList();
        }
        return View(new ItemIndexViewModel { Items = items.ToList(), SearchQuery = searchQuery, ShowExpiredOnly = showExpiredOnly });
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

        return View(new AdminPatientsIndexViewModel
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
        SubstancesText = string.Join(Environment.NewLine, item.ActiveSubstances.Select(e => $"{e.Key}:{e.Value}")), 
        BatchesText = string.Join(Environment.NewLine, item.Batches.Select(e => $"{e.Key:yyyy-MM-dd}:{e.Value}")) 
    };
    private static Dictionary<string, float> ParseSubstancesText(string text) => string.IsNullOrWhiteSpace(text) ? new() : text.Split(LineDelimiters, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Split(':')).ToDictionary(p => p[0].Trim(), p => float.Parse(p[1]));
    private static Dictionary<DateOnly, int> ParseBatchesText(string text) => string.IsNullOrWhiteSpace(text) ? new() : text.Split(LineDelimiters, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Split(':')).ToDictionary(p => DateOnly.Parse(p[0]), p => int.Parse(p[1]));
    private static PatientListItemViewModel MapPatientListItem(Patient p) => new() { Id = p.PatientId, FirstName = p.FirstName, LastName = p.LastName, Cnp = p.Cnp, Dob = p.DateOfBirth, Sex = p.Sex.ToString(), PhoneNo = FormatPhoneNumber(p.PhoneNumber), EmergencyContact = FormatPhoneNumber(p.EmergencyContact), IsArchived = p.IsArchived, IsDeceased = p.DateOfDeath.HasValue };
    private static EditPatientViewModel MapEditPatient(Patient p, PatientListItemViewModel? _) => new() { Id = p.PatientId, FirstName = p.FirstName, LastName = p.LastName, Cnp = p.Cnp, Dob = p.DateOfBirth, Dod = p.DateOfDeath, Sex = p.Sex, PhoneNo = CompactPhoneNumber(p.PhoneNumber), EmergencyContact = CompactEmergencyContact(p.EmergencyContact), IsArchived = p.IsArchived, IsDonor = p.IsDonor, Transferred = p.Transferred };
    private static string NormalizePhone(string phone) => string.IsNullOrWhiteSpace(phone) ? phone : phone.Replace(" ", "").Replace("-", "");
    private static string FormatPhoneNumber(string phone) => string.IsNullOrWhiteSpace(phone) || phone.Length != 10 ? phone : $"+40 {phone.Substring(1, 3)} {phone.Substring(4, 3)} {phone.Substring(7, 3)}";
    private static string CompactPhoneNumber(string phone) => string.IsNullOrWhiteSpace(phone) ? phone : (phone.StartsWith("0") && phone.Length == 10 ? $"+40{phone[1..]}" : phone);
    private static string CompactEmergencyContact(string contact) => string.IsNullOrWhiteSpace(contact) ? contact : string.Join(",", contact.Split(',').Select(p => p.Trim()).Select(p => p.Any(char.IsDigit) ? CompactPhoneNumber(p) : p));
    private static List<string> SplitConditions(string? text) => string.IsNullOrWhiteSpace(text) ? new() : text.Split(',').Select(c => c.Trim()).Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
    private async Task<List<Patient>> SearchPatientsAsync(string? q, int? min, int? max, Sex? s, CancellationToken ct) => await patientService.SearchPatientsAsync(new SearchPatientsRequest { MinAge = min, MaxAge = max, Sex = s, Cnp = (q?.All(char.IsDigit) == true && q.Length == 13) ? q : null, NamePart = (q?.All(char.IsDigit) == false) ? q : null }, ct);
    private IActionResult RedirectToLogin() => RedirectToAction("Login", "Auth");
}