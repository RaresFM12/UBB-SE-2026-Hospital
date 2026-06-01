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

    public AdminController(
        IAdminService adminService,
        IPatientService patientService,
        IAllergyService allergyService)
    {
        this.adminService = adminService;
        this.patientService = patientService;
        this.allergyService = allergyService;
    }

    public IActionResult Index(string searchQuery = "", bool showExpiredOnly = false)
    {
        List<Item> items = this.LoadItems(searchQuery, showExpiredOnly);
        var viewModel = new ItemIndexViewModel
        {
            Items = items,
            SearchQuery = searchQuery,
            ShowExpiredOnly = showExpiredOnly,
        };
        return this.View(viewModel);
    }

    public IActionResult ItemDetails(int id)
    {
        Item item = this.adminService.GetItemByIdAsync(id).Result;
        if (item == null) return this.NotFound();
        return this.View(item);
    }

    [HttpGet]
    public IActionResult Create() => this.View(new ItemViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ItemViewModel viewModel)
    {
        if (!this.ModelState.IsValid) return this.View(viewModel);

        Dictionary<string, float> activeSubstances = ParseSubstancesText(viewModel.SubstancesText);
        Dictionary<DateOnly, int> batches = ParseBatchesText(viewModel.BatchesText);

        var newItem = new Item(
            viewModel.Name, viewModel.Producer, viewModel.Category,
            viewModel.Price, viewModel.NumberOfPills, activeSubstances, batches,
            viewModel.Quantity, viewModel.Label, viewModel.Description,
            viewModel.ImagePath, viewModel.DiscountPercentage, viewModel.Quantity);

        try
        {
            this.adminService.AddItem(newItem);
            return this.RedirectToAction(nameof(this.Index));
        }
        catch (ArgumentException ex)
        {
            this.ModelState.AddModelError(string.Empty, ex.Message);
            return this.View(viewModel);
        }
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        Item item = this.adminService.GetItemById(id);
        if (item == null) return this.NotFound();
        return this.View(MapItemToViewModel(item));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, ItemViewModel viewModel)
    {
        if (!this.ModelState.IsValid) return this.View(viewModel);

        Dictionary<string, float> activeSubstances = ParseSubstancesText(viewModel.SubstancesText);
        Dictionary<DateOnly, int> batches = ParseBatchesText(viewModel.BatchesText);

        var updatedItem = new Item(
            id, viewModel.Name, viewModel.Producer, viewModel.Category,
            viewModel.Price, viewModel.NumberOfPills, viewModel.Label,
            viewModel.Description, viewModel.ImagePath, viewModel.DiscountPercentage,
            viewModel.Quantity)
        {
            ActiveSubstances = activeSubstances,
            Batches = batches,
        };

        try
        {
            this.adminService.UpdateItemById(id, updatedItem);
            return this.RedirectToAction(nameof(this.Index));
        }
        catch (ArgumentException ex)
        {
            this.ModelState.AddModelError(string.Empty, ex.Message);
            return this.View(viewModel);
        }
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        Item item = this.adminService.GetItemById(id);
        if (item == null) return this.NotFound();
        return this.View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        this.adminService.RemoveItemById(id);
        return this.RedirectToAction(nameof(this.Index));
    }

    [HttpGet]
    public IActionResult Substances()
    {
        List<Substance> substances = this.adminService.GetAllSubstances();
        return this.View(substances);
    }

    [HttpGet]
    public IActionResult CreateSubstance() => this.View(new SubstanceViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateSubstance(SubstanceViewModel viewModel)
    {
        if (!this.ModelState.IsValid) return this.View(viewModel);

        var newSubstance = new Substance(viewModel.Name, viewModel.LethalDose, viewModel.Description);
        try
        {
            this.adminService.AddSubstance(newSubstance);
            return this.RedirectToAction(nameof(this.Substances));
        }
        catch (ArgumentException ex)
        {
            this.ModelState.AddModelError(string.Empty, ex.Message);
            return this.View(viewModel);
        }
    }

    [HttpGet]
    public IActionResult EditSubstance(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return this.NotFound();
        Substance substance = this.adminService.GetSubstanceByName(name);
        if (substance == null) return this.NotFound();

        return this.View(new SubstanceViewModel
        {
            Name = substance.Name,
            LethalDose = substance.LethalDose,
            Description = substance.Description,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditSubstance(string name, SubstanceViewModel viewModel)
    {
        if (!this.ModelState.IsValid) return this.View(viewModel);

        try
        {
            this.adminService.UpdateSubstanceByName(name, new Substance(viewModel.Name, viewModel.LethalDose, viewModel.Description));
            return this.RedirectToAction(nameof(this.Substances));
        }
        catch (ArgumentException ex)
        {
            this.ModelState.AddModelError(string.Empty, ex.Message);
            return this.View(viewModel);
        }
    }

    [HttpGet]
    public IActionResult DeleteSubstance(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return this.NotFound();
        Substance substance = this.adminService.GetSubstanceByName(name);
        if (substance == null) return this.NotFound();
        return this.View(substance);
    }

    [HttpPost, ActionName("DeleteSubstance")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteSubstanceConfirmed(string name)
    {
        Substance substance = this.adminService.GetSubstanceByName(name);
        if (substance != null) this.adminService.RemoveSubstanceByName(substance);
        return this.RedirectToAction(nameof(this.Substances));
    }

    [HttpGet]
    public IActionResult Statistics()
    {
        List<Tuple<int, string, int>> rawTopItems = this.adminService.GetTop30Items();
        Dictionary<string, int> topSubstances = this.adminService.GetTop30Substances();

        var statisticsViewModel = new AdminStatisticsViewModel
        {
            TopItems = rawTopItems.ConvertAll(t => new TopItemViewModel
            {
                ItemId = t.Item1,
                ItemName = t.Item2,
                OrderCount = t.Item3,
            }),
            TopSubstances = topSubstances,
        };

        return this.View(statisticsViewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Patients(
        string? searchQuery,
        int? minAge,
        int? maxAge,
        Sex? sex,
        bool archived = false,
        int? selectedId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<Patient> searchResults = await SearchPatientsAsync(searchQuery, minAge, maxAge, sex, cancellationToken);

            List<Patient> visiblePatients = searchResults
                .Where(p => p.IsArchived == archived)
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToList();

            Patient? selectedPatient = null;
            if (selectedId.HasValue)
            {
                selectedPatient = visiblePatients.FirstOrDefault(p => p.Id == selectedId.Value)
                    ?? await patientService.GetByIdAsync(selectedId.Value, cancellationToken);

                if (selectedPatient?.IsArchived != archived) selectedPatient = null;
            }

            List<PatientListItemViewModel> patientRows = visiblePatients.Select(MapPatientListItem).ToList();
            PatientListItemViewModel? selectedPatientRow = selectedPatient is null
                ? null
                : patientRows.FirstOrDefault(p => p.Id == selectedPatient.Id);

            EditPatientViewModel? selectedPatientModel = selectedPatient is null
                ? null
                : MapEditPatient(selectedPatient, selectedPatientRow);

            var model = new AdminPatientsIndexViewModel
            {
                SearchQuery = searchQuery,
                MinAge = minAge,
                MaxAge = maxAge,
                Sex = sex,
                ShowArchived = archived,
                SelectedPatientId = selectedPatient?.Id,
                Patients = patientRows,
                SelectedPatient = selectedPatientModel
            };

            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return View(new AdminPatientsIndexViewModel
            {
                SearchQuery = searchQuery,
                MinAge = minAge,
                MaxAge = maxAge,
                Sex = sex,
                ShowArchived = archived
            });
        }
    }

    [HttpGet]
    public IActionResult CreatePatient() => View("~/Views/Patients/Create.cshtml", new CreatePatientViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePatient(CreatePatientViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View("~/Views/Patients/Create.cshtml", model);

        var dto = new CreatePatientDto
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Cnp = model.Cnp.Trim(),
            Dob = model.Dob,
            Sex = model.Sex,
            PhoneNo = NormalizePhone(model.PhoneNo),
            EmergencyContact = NormalizePhone(model.EmergencyContact),
            IsDonor = false
        };

        try
        {
            Patient created = await patientService.CreatePatientAsync(dto, cancellationToken);
            TempData["SuccessMessage"] = $"Patient {created.FullName} was created successfully.";
            return RedirectToAction(nameof(CreateMedicalHistory), new { patientId = created.Id });
        }
        catch (UnauthorizedAccessException) { return RedirectToLogin(); }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("~/Views/Patients/Create.cshtml", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> CreateMedicalHistory(int patientId, CancellationToken cancellationToken)
    {
        try
        {
            Patient? patient = await patientService.GetByIdAsync(patientId, cancellationToken);
            if (patient is null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction(nameof(Patients));
            }
            return View(await BuildMedicalHistoryModelAsync(patient, null, cancellationToken));
        }
        catch (UnauthorizedAccessException) { return RedirectToLogin(); }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMedicalHistory(CreateMedicalHistoryViewModel model, CancellationToken cancellationToken)
    {
        Patient? patient;
        try { patient = await patientService.GetByIdAsync(model.PatientId, cancellationToken); }
        catch (UnauthorizedAccessException) { return RedirectToLogin(); }

        if (patient is null)
        {
            TempData["ErrorMessage"] = "Patient not found.";
            return RedirectToAction(nameof(Patients));
        }

        if (!ModelState.IsValid) return View(await BuildMedicalHistoryModelAsync(patient, model, cancellationToken));

        var dto = new CreateMedicalHistoryDto
        {
            BloodType = model.BloodType,
            Rh = model.Rh,
            ChronicConditions = SplitConditions(model.ChronicConditionsText),
            AllergyIds = model.AllergyIds.Distinct().ToList()
        };

        try
        {
            await patientService.CreateMedicalHistoryAsync(model.PatientId, dto, cancellationToken);
            TempData["SuccessMessage"] = "Patient and medical history saved successfully.";
            return RedirectToAction(nameof(Patients), new { selectedId = model.PatientId });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await BuildMedicalHistoryModelAsync(patient, model, cancellationToken));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SkipMedicalHistory(int patientId)
    {
        TempData["SuccessMessage"] = "Patient added successfully.";
        return RedirectToAction(nameof(Patients), new { selectedId = patientId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePatient(
        EditPatientViewModel model, string? searchQuery, int? minAge, int? maxAge,
        Sex? filterSex, bool archived, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please correct the selected patient form and try again.";
            return RedirectToAction(nameof(Patients), new { searchQuery, minAge, maxAge, sex = filterSex, archived, selectedId = model.Id });
        }

        var dto = new UpdatePatientRequest
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Cnp = model.Cnp,
            Dob = model.Dob,
            Dod = model.Dod,
            Sex = model.Sex,
            PhoneNo = NormalizePhone(model.PhoneNo),
            EmergencyContact = NormalizePhone(model.EmergencyContact),
            IsArchived = model.IsArchived,
            IsDonor = model.IsDonor,
            Transferred = model.Transferred
        };

        try
        {
            await patientService.UpdatePatientAsync(model.Id, dto, cancellationToken);
            TempData["SuccessMessage"] = "Patient updated successfully.";
        }
        catch (UnauthorizedAccessException) { return RedirectToLogin(); }
        catch (ArgumentException ex) { TempData["ErrorMessage"] = ex.Message; }

        return RedirectToAction(nameof(Patients), new { searchQuery, minAge, maxAge, sex = filterSex, archived, selectedId = model.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchivePatient(
        int id, string? searchQuery, int? minAge, int? maxAge,
        Sex? filterSex, bool archived, CancellationToken cancellationToken)
    {
        try
        {
            Patient? patient = await patientService.GetByIdAsync(id, cancellationToken);
            if (patient is null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction(nameof(Patients), new { searchQuery, minAge, maxAge, sex = filterSex, archived });
            }
            await patientService.ArchivePatientAsync(id, cancellationToken);
            TempData["SuccessMessage"] = $"Archived {patient.FullName}.";
            return RedirectToAction(nameof(Patients), new { searchQuery, minAge, maxAge, sex = filterSex, archived = true, selectedId = id });
        }
        catch (UnauthorizedAccessException) { return RedirectToLogin(); }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DearchivePatient(
        int id, string? searchQuery, int? minAge, int? maxAge,
        Sex? filterSex, bool archived, CancellationToken cancellationToken)
    {
        try
        {
            await patientService.DearchivePatientAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Patient restored to active records.";
            return RedirectToAction(nameof(Patients), new { searchQuery, minAge, maxAge, sex = filterSex, archived = false, selectedId = id });
        }
        catch (UnauthorizedAccessException) { return RedirectToLogin(); }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Patients), new { searchQuery, minAge, maxAge, sex = filterSex, archived });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsDeceased(
        int id, DateTime? deathDate, string? searchQuery, int? minAge, int? maxAge,
        Sex? filterSex, bool archived, CancellationToken cancellationToken)
    {
        if (!deathDate.HasValue)
        {
            TempData["ErrorMessage"] = "Please choose a date of death.";
            return RedirectToAction(nameof(Patients), new { searchQuery, minAge, maxAge, sex = filterSex, archived, selectedId = id });
        }

        try
        {
            await patientService.ArchiveAsDeceasedAsync(id, new ArchiveAsDeceasedDto { DeathDate = deathDate.Value }, cancellationToken);
            TempData["SuccessMessage"] = "Patient marked as deceased.";
            return RedirectToAction(nameof(Patients), new { searchQuery, minAge, maxAge, sex = filterSex, archived = true, selectedId = id });
        }
        catch (UnauthorizedAccessException) { return RedirectToLogin(); }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Patients), new { searchQuery, minAge, maxAge, sex = filterSex, archived, selectedId = id });
        }
    }

    [HttpGet]
    public IActionResult PatientDetails(int id) => RedirectToAction("Details", "Patient", new { id });

    private List<Item> LoadItems(string searchQuery, bool showExpiredOnly)
    {
        if (showExpiredOnly) return this.adminService.GetExpiredItems();
        if (!string.IsNullOrWhiteSpace(searchQuery)) return this.adminService.SearchItemsByName(searchQuery);
        return this.adminService.GetAllItems();
    }

    private static ItemViewModel MapItemToViewModel(Item item)
    {
        string FormatSubstanceEntry(KeyValuePair<string, float> e) =>
            $"{e.Key}{SubstanceLineDelimiter}{e.Value.ToString(CultureInfo.InvariantCulture)}";
        string FormatBatchEntry(KeyValuePair<DateOnly, int> e) =>
            $"{e.Key.ToString(BatchDateFormat, CultureInfo.InvariantCulture)}{SubstanceLineDelimiter}{e.Value}";

        return new ItemViewModel
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
            SubstancesText = string.Join(Environment.NewLine, item.ActiveSubstances.Select(FormatSubstanceEntry)),
            BatchesText = string.Join(Environment.NewLine, item.Batches.Select(FormatBatchEntry)),
        };
    }

    private static Dictionary<string, float> ParseSubstancesText(string substancesText)
    {
        var result = new Dictionary<string, float>();
        if (string.IsNullOrWhiteSpace(substancesText)) return result;

        foreach (string line in substancesText.Split(LineDelimiters, StringSplitOptions.RemoveEmptyEntries))
        {
            string[] parts = line.Split(SubstanceLineDelimiter, 2);
            if (parts.Length == 2 && float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float concentration))
                result[parts[0].Trim()] = concentration;
        }
        return result;
    }

    private static Dictionary<DateOnly, int> ParseBatchesText(string batchesText)
    {
        var result = new Dictionary<DateOnly, int>();
        if (string.IsNullOrWhiteSpace(batchesText)) return result;

        foreach (string line in batchesText.Split(LineDelimiters, StringSplitOptions.RemoveEmptyEntries))
        {
            string[] parts = line.Split(SubstanceLineDelimiter, 2);
            if (parts.Length == 2
                && DateOnly.TryParseExact(parts[0].Trim(), BatchDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date)
                && int.TryParse(parts[1].Trim(), out int packs))
                result[date] = packs;
        }
        return result;
    }

    private async Task<List<Patient>> SearchPatientsAsync(string? searchQuery, int? minAge, int? maxAge, Sex? sex, CancellationToken cancellationToken)
    {
        var dto = new SearchPatientsDto { MinAge = minAge, MaxAge = maxAge, Sex = sex };

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            string trimmed = searchQuery.Trim();
            if (trimmed.All(char.IsDigit) && trimmed.Length == 13) dto.Cnp = trimmed;
            else dto.NamePart = trimmed;
        }

        return await patientService.SearchPatientsAsync(dto, cancellationToken);
    }

    private static PatientListItemViewModel MapPatientListItem(Patient patient) => new()
    {
        Id = patient.Id,
        FirstName = patient.FirstName,
        LastName = patient.LastName,
        Cnp = patient.Cnp,
        Dob = patient.Dob,
        Sex = patient.Sex.ToString(),
        PhoneNo = FormatPhoneNumber(patient.PhoneNo),
        EmergencyContact = FormatPhoneNumber(patient.EmergencyContact),
        IsArchived = patient.IsArchived,
        IsDeceased = patient.IsDeceased
    };

    private static EditPatientViewModel MapEditPatient(Patient patient, PatientListItemViewModel? _) => new()
    {
        Id = patient.Id,
        FirstName = patient.FirstName,
        LastName = patient.LastName,
        Cnp = patient.Cnp,
        Dob = patient.Dob,
        Dod = patient.Dod,
        Sex = patient.Sex,
        PhoneNo = CompactPhoneNumber(patient.PhoneNo),
        EmergencyContact = CompactEmergencyContact(patient.EmergencyContact),
        IsArchived = patient.IsArchived,
        IsDonor = patient.IsDonor,
        Transferred = patient.Transferred
    };

    private static string NormalizePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return phone;
        string normalized = phone.Replace(" ", string.Empty, StringComparison.Ordinal)
                                 .Replace("-", string.Empty, StringComparison.Ordinal);
        return normalized.StartsWith("+40", StringComparison.Ordinal) ? $"0{normalized[3..]}" : normalized;
    }

    private static string FormatPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return phone;
        string normalized = NormalizePhone(phone);
        if (!normalized.StartsWith('0') || normalized.Length != 10) return phone;
        return $"+40 {normalized.Substring(1, 3)} {normalized.Substring(4, 3)} {normalized.Substring(7, 3)}";
    }

    private static string CompactPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return phone;
        string normalized = NormalizePhone(phone);
        return normalized.StartsWith('0') && normalized.Length == 10 ? $"+40{normalized[1..]}" : normalized;
    }

    private static string CompactEmergencyContact(string contact)
    {
        if (string.IsNullOrWhiteSpace(contact)) return contact;
        return string.Join(",", contact.Split(',', StringSplitOptions.None)
            .Select(p => { string t = p.Trim(); return t.Any(char.IsDigit) ? CompactPhoneNumber(t) : t; }));
    }

    private async Task<CreateMedicalHistoryViewModel> BuildMedicalHistoryModelAsync(
        Patient patient, CreateMedicalHistoryViewModel? source, CancellationToken cancellationToken)
    {
        List<AllergyOptionViewModel> allergies = (await allergyService.GetAllergiesAsync(cancellationToken))
            .OrderBy(a => a.AllergyName)
            .Select(a => new AllergyOptionViewModel { Id = a.Id, Name = a.AllergyName })
            .ToList();

        return new CreateMedicalHistoryViewModel
        {
            PatientId = patient.Id,
            PatientName = patient.FullName,
            BloodType = source?.BloodType ?? BloodType.A,
            Rh = source?.Rh ?? Rh.Positive,
            ChronicConditionsText = source?.ChronicConditionsText ?? string.Empty,
            AllergyIds = source?.AllergyIds ?? new List<int>(),
            AvailableAllergies = allergies
        };
    }

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before opening patient administration.";
        return RedirectToAction("AuthenticationView", "AuthController");
    }

    private static List<string> SplitConditions(string? text) =>
        string.IsNullOrWhiteSpace(text)
            ? new List<string>()
            : text.Split(',', StringSplitOptions.RemoveEmptyEntries)
                  .Select(c => c.Trim())
                  .Where(c => !string.IsNullOrWhiteSpace(c))
                  .ToList();
}