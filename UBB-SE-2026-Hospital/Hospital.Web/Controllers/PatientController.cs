using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Web.Models.Admin;
using Hospital.Web.Models.Patients;
using Hospital.Shared.Services;
using Hospital.Web.Models.Patient;
using Patient = Hospital.Shared.Models.PatientEr.Patient;
using MedicalRecord = Hospital.Shared.Models.PatientEr.MedicalRecord;
using PatientProfileViewModel = Hospital.Web.Models.Patient.PatientProfileViewModel;
using DbPatient = Hospital.Data.Models.Patient;
using Hospital.Services.PatientEr;

namespace Hospital.Web.Controllers;

[Authorize]
public class PatientsController : Controller
{
    private const string EmergencyRoomRecordSource = "ER";
    private const string MissingValueLabel = "N/A";
    private const int MissingStaffId = 0;
    private const int NoDiscountApplied = 0;

    private readonly IPatientService _patientService;
    private readonly IAllergyService _allergyService;

    public PatientsController(IPatientService patientService, IAllergyService allergyService)
    {
        _patientService = patientService;
        _allergyService = allergyService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int? minAge, int? maxAge, Sex? sex, bool archived = false, int? selectedId = null, bool create = false, CancellationToken cancellationToken = default)
    {
        return View(await BuildIndexViewModelAsync(search, minAge, maxAge, sex, archived, selectedId, create, null, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> PatientProfile(int id, int? selectedRecordId, CancellationToken cancellationToken = default)
    {
        try
        {
            PatientProfileViewModel model = await BuildProfileAsync(id, selectedRecordId, cancellationToken);
            ApplyTemporaryDiscount(model);
            return View("~/Views/PatientProfile/Details.cshtml", model);
        }
        catch (UnauthorizedAccessException) { return RedirectToLogin(); }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index", "Admin");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePatient(EditPatientViewModel model, string? searchQuery, int? minAge, int? maxAge, Sex? filterSex, bool archived, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please correct the patient details and try again.";
            return RedirectToAction(nameof(Index), BuildIndexRouteValues(searchQuery, minAge, maxAge, filterSex, archived, model.Id));
        }

        try
        {
            await _patientService.UpdatePatientAsync(new DbPatient
            {
                PatientId = model.Id,
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Cnp = model.Cnp.Trim(),
                DateOfBirth = model.Dob,
                DateOfDeath = model.Dod,
                Sex = model.Sex,
                PhoneNumber = model.PhoneNo.Trim(),
                EmergencyContact = model.EmergencyContact.Trim(),
                IsArchived = model.IsArchived,
                IsDonor = model.IsDonor,
                Transferred = model.Transferred
            }, cancellationToken);

            TempData["SuccessMessage"] = "Patient details updated successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), BuildIndexRouteValues(searchQuery, minAge, maxAge, filterSex, archived, model.Id));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePatient([Bind(Prefix = "NewPatient")] CreatePatientViewModel model, string? searchQuery, int? minAge, int? maxAge, Sex? filterSex, bool archived, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return View("Index", await BuildIndexViewModelAsync(searchQuery, minAge, maxAge, filterSex, archived, null, true, model, cancellationToken));

        try
        {
            var patient = await _patientService.CreatePatientAsync(new CreatePatientRequest
            {
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Cnp = model.Cnp.Trim(),
                DateOfBirth = model.Dob,
                Sex = model.Sex,
                PhoneNumber = model.PhoneNo.Trim(),
                EmergencyContact = model.EmergencyContact.Trim(),
                IsDonor = false
            }, cancellationToken);

            bool hasMedicalHistoryData = model.BloodType.HasValue ||
                model.Rh.HasValue ||
                !string.IsNullOrWhiteSpace(model.ChronicConditionsText) ||
                model.AllergyIds.Count > 0;

            if (hasMedicalHistoryData)
            {
                await _patientService.CreateMedicalHistoryAsync(patient.PatientId, new CreateMedicalHistoryRequest
                {
                    BloodType = model.BloodType,
                    Rh = model.Rh,
                    ChronicConditions = SplitConditions(model.ChronicConditionsText),
                    AllergyIds = model.AllergyIds
                }, cancellationToken);
            }

            TempData["SuccessMessage"] = $"Patient {patient.FullName} was created successfully.";
            return RedirectToAction(nameof(Index), BuildIndexRouteValues(searchQuery, minAge, maxAge, filterSex, archived, patient.PatientId, false));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Index", await BuildIndexViewModelAsync(searchQuery, minAge, maxAge, filterSex, archived, null, true, model, cancellationToken));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchivePatient(int id, string? searchQuery, int? minAge, int? maxAge, Sex? filterSex, CancellationToken cancellationToken = default)
    {
        try
        {
            await _patientService.ArchivePatientAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Patient archived successfully.";
            return RedirectToAction(nameof(Index), BuildIndexRouteValues(searchQuery, minAge, maxAge, filterSex, true, id));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index), BuildIndexRouteValues(searchQuery, minAge, maxAge, filterSex, false, id));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DearchivePatient(int id, string? searchQuery, int? minAge, int? maxAge, Sex? filterSex, CancellationToken cancellationToken = default)
    {
        try
        {
            await _patientService.DearchivePatientAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Patient restored to active successfully.";
            return RedirectToAction(nameof(Index), BuildIndexRouteValues(searchQuery, minAge, maxAge, filterSex, false, id));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index), BuildIndexRouteValues(searchQuery, minAge, maxAge, filterSex, true, id));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsDeceased(int id, DateTime? deathDate, string? searchQuery, int? minAge, int? maxAge, Sex? filterSex, CancellationToken cancellationToken = default)
    {
        if (!deathDate.HasValue)
        {
            TempData["ErrorMessage"] = "Please provide a date of death.";
            return RedirectToAction(nameof(Index), BuildIndexRouteValues(searchQuery, minAge, maxAge, filterSex, true, id));
        }

        try
        {
            await _patientService.ArchiveAsDeceasedAsync(id, deathDate.Value, cancellationToken);
            TempData["SuccessMessage"] = "Patient marked as deceased.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), BuildIndexRouteValues(searchQuery, minAge, maxAge, filterSex, true, id));
    }

    private async Task<PatientProfileViewModel> BuildProfileAsync(int id, int? selectedRecordId, CancellationToken cancellationToken)
    {
        Patient patient = await _patientService.GetPatientDetailsAsync(id, cancellationToken);
        List<PatientRecordViewModel> records = new List<PatientRecordViewModel>();

        if (patient.MedicalHistory?.MedicalRecords != null)
        {
            records = patient.MedicalHistory.MedicalRecords
                .OrderByDescending(r => r.ConsultationDate)
                .Select(MapRecord)
                .ToList();
        }

        var selectedRecord = selectedRecordId.HasValue ? records.FirstOrDefault(r => r.Id == selectedRecordId.Value) : records.FirstOrDefault();

        var mh = patient.MedicalHistory;

        var model = new PatientProfileViewModel
        {
            Id = patient.PatientId,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Dob = patient.DateOfBirth,
            Sex = patient.Sex.ToString(),
            Cnp = patient.Cnp,
            PhoneNo = patient.PhoneNo,
            EmergencyContact = patient.EmergencyContact,
            IsArchived = patient.IsArchived,
            BloodType = mh?.BloodType?.ToString() ?? "N/A",
            Rh = mh?.Rh?.ToString() ?? "N/A",
            ChronicConditions = mh?.ChronicConditions?.Any() == true
                ? string.Join(", ", mh.ChronicConditions)
                : "None",
            Allergies = mh?.Allergies?.Select(a => $"{a.Allergy?.AllergyName ?? "Unknown"} ({a.SeverityLevel})").ToList() ?? new(),
            MedicalRecords = records,
            SelectedRecordId = selectedRecord?.Id,
            SelectedRecord = selectedRecord
        };

        // Load the prescription for the selected record so the "View Prescription"
        // button can link to it (the detail page loads the full prescription itself).
        if (selectedRecord is not null)
        {
            try
            {
                var prescription = await _patientService.GetPrescriptionByRecordIdAsync(selectedRecord.Id, cancellationToken);
                if (prescription is not null)
                {
                    model.SelectedPrescription = new PatientPrescriptionViewModel { Id = prescription.PrescriptionId };
                }
            }
            catch
            {
                // Prescription is optional — leave it null if it can't be loaded.
            }
        }

        return model;
    }

    private static PatientRecordViewModel MapRecord(MedicalRecord record)
    {
        return new PatientRecordViewModel
        {
            Id = record.RecordId,
            ConsultationDate = record.ConsultationDate,
            SourceType = EmergencyRoomRecordSource,
            StaffId = record.StaffMember?.StaffId ?? MissingStaffId,
            Symptoms = record.Symptoms ?? MissingValueLabel,
            Diagnosis = record.Diagnosis ?? MissingValueLabel,
            BasePrice = record.BasePrice,
            FinalPrice = record.FinalPrice,
            DiscountApplied = record.DiscountApplied ?? NoDiscountApplied
        };
    }

    private void ApplyTemporaryDiscount(PatientProfileViewModel model)
    {
        if (TempData["TemporaryDiscount"] is string discountText && int.TryParse(discountText, out int discount))
            model.TemporaryDiscount = discount;

        if (TempData["TemporaryDiscountedPrice"] is string priceText && decimal.TryParse(priceText, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out decimal discountedPrice))
        {
            model.TemporaryDiscountedPrice = discountedPrice;
            model.FinalPrice = discountedPrice;
        }
    }

    private static int GetAge(DateTime dob)
    {
        var today = DateTime.Today;
        int age = today.Year - dob.Year;
        if (dob.Date > today.AddYears(-age)) age--;
        return age;
    }

    private async Task<PatientsIndexViewModel> BuildIndexViewModelAsync(
        string? search,
        int? minAge,
        int? maxAge,
        Sex? sex,
        bool archived,
        int? selectedId,
        bool create,
        CreatePatientViewModel? createPatient,
        CancellationToken cancellationToken)
    {
        var searchResults = await SearchPatientsAsync(search, minAge, maxAge, sex, cancellationToken);
        var visiblePatients = searchResults
            .Where(p => p.IsArchived == archived)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToList();

        DbPatient? selectedPatient = selectedId.HasValue && !create
            ? visiblePatients.FirstOrDefault(p => p.PatientId == selectedId.Value) ?? await _patientService.GetByIdAsync(selectedId.Value, cancellationToken)
            : null;

        var newPatient = createPatient ?? new CreatePatientViewModel();
        await PopulateAvailableAllergiesAsync(newPatient);

        return new PatientsIndexViewModel
        {
            SearchQuery = search,
            MinAge = minAge,
            MaxAge = maxAge,
            Sex = sex,
            ShowArchived = archived,
            SelectedPatientId = selectedPatient?.PatientId,
            ShowCreateForm = create,
            Patients = visiblePatients.Select(MapPatientListItem).ToList(),
            SelectedPatient = selectedPatient is null ? null : MapEditPatient(selectedPatient),
            NewPatient = newPatient
        };
    }

    private async Task<List<DbPatient>> SearchPatientsAsync(string? q, int? min, int? max, Sex? s, CancellationToken ct) =>
        await _patientService.SearchPatientsAsync(new Hospital.Data.Models.DTOs.SearchPatientsRequest
        {
            MinAge = min,
            MaxAge = max,
            Sex = s,
            Cnp = q?.All(char.IsDigit) == true && q.Length == 13 ? q : null,
            NamePart = q?.All(char.IsDigit) == false ? q : null
        }, ct);

    private async Task PopulateAvailableAllergiesAsync(CreatePatientViewModel model)
    {
        var allergies = await _allergyService.GetAllergiesAsync();
        model.AvailableAllergies = allergies
            .OrderBy(allergy => allergy.AllergyName)
            .Select(allergy => new AllergyOptionViewModel
            {
                Id = allergy.AllergyId,
                Name = allergy.AllergyName
            })
            .ToList();
    }

    private static PatientListItemViewModel MapPatientListItem(DbPatient patient) => new()
    {
        Id = patient.PatientId,
        FirstName = patient.FirstName,
        LastName = patient.LastName,
        Cnp = patient.Cnp,
        Dob = patient.DateOfBirth,
        Sex = patient.Sex.ToString(),
        PhoneNo = FormatPhoneNumber(patient.PhoneNumber),
        EmergencyContact = FormatEmergencyContact(patient.EmergencyContact),
        IsArchived = patient.IsArchived,
        IsDeceased = patient.IsDeceased
    };

    private static EditPatientViewModel MapEditPatient(DbPatient patient) => new()
    {
        Id = patient.PatientId,
        FirstName = patient.FirstName,
        LastName = patient.LastName,
        Cnp = patient.Cnp,
        Dob = patient.DateOfBirth,
        Dod = patient.DateOfDeath,
        Sex = patient.Sex,
        PhoneNo = patient.PhoneNumber,
        EmergencyContact = patient.EmergencyContact,
        IsArchived = patient.IsArchived,
        IsDonor = patient.IsDonor,
        Transferred = patient.Transferred
    };

    private static List<string> SplitConditions(string? text) =>
        string.IsNullOrWhiteSpace(text)
            ? new List<string>()
            : text.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(condition => condition.Trim())
                .Where(condition => !string.IsNullOrWhiteSpace(condition))
                .ToList();

    private static object BuildIndexRouteValues(string? searchQuery, int? minAge, int? maxAge, Sex? filterSex, bool archived, int? selectedId, bool create = false) =>
        new
        {
            search = searchQuery,
            minAge,
            maxAge,
            sex = filterSex,
            archived,
            selectedId,
            create
        };

    private static string FormatPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return phone;

        string normalized = phone.Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);

        if (normalized.StartsWith("+40", StringComparison.Ordinal) && normalized.Length == 12)
            normalized = $"0{normalized[3..]}";

        if (!normalized.StartsWith('0') || normalized.Length != 10)
            return phone;

        return $"+40 {normalized.Substring(1, 3)} {normalized.Substring(4, 3)} {normalized.Substring(7, 3)}";
    }

    private static string FormatEmergencyContact(string contact)
    {
        if (string.IsNullOrWhiteSpace(contact))
            return contact;

        string[] parts = contact.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return contact;

        string lastPart = parts[^1];
        string formattedPhone = FormatPhoneNumber(lastPart);
        return formattedPhone == lastPart
            ? contact
            : $"{string.Join(' ', parts[..^1])} {formattedPhone}".Trim();
    }

    private IActionResult RedirectToLogin() => RedirectToAction("Login", "Auth");
}
