using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Hospital.Data.Models;
using Hospital.Web.Models.Admin;
using Hospital.Web.Models.Patients;
using Hospital.Shared.Services;
using Hospital.Web.Models.Patient;
using Patient = Hospital.Shared.Models.PatientEr.Patient;
using MedicalRecord = Hospital.Shared.Models.PatientEr.MedicalRecord;
using PatientProfileViewModel = Hospital.Web.Models.Patient.PatientProfileViewModel;
using DbPatient = Hospital.Data.Models.Patient;

namespace Hospital.Web.Controllers;

[Authorize]
public class PatientsController : Controller
{
    private const string EmergencyRoomRecordSource = "ER";
    private const string MissingValueLabel = "N/A";
    private const int MissingStaffId = 0;
    private const int NoDiscountApplied = 0;

    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int? minAge, int? maxAge, Sex? sex, bool archived = false, int? selectedId = null, CancellationToken cancellationToken = default)
    {
        var searchResults = await SearchPatientsAsync(search, minAge, maxAge, sex, cancellationToken);
        var visiblePatients = searchResults
            .Where(p => p.IsArchived == archived)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToList();

        DbPatient? selectedPatient = selectedId.HasValue
            ? visiblePatients.FirstOrDefault(p => p.PatientId == selectedId.Value) ?? await _patientService.GetByIdAsync(selectedId.Value, cancellationToken)
            : null;

        return View(new PatientsIndexViewModel
        {
            SearchQuery = search,
            MinAge = minAge,
            MaxAge = maxAge,
            Sex = sex,
            ShowArchived = archived,
            SelectedPatientId = selectedPatient?.PatientId,
            Patients = visiblePatients.Select(MapPatientListItem).ToList(),
            SelectedPatient = selectedPatient is null ? null : MapEditPatient(selectedPatient)
        });
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

        return new PatientProfileViewModel
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
            MedicalRecords = records,
            SelectedRecordId = selectedRecord?.Id,
            SelectedRecord = selectedRecord
        };
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

    private async Task<List<DbPatient>> SearchPatientsAsync(string? q, int? min, int? max, Sex? s, CancellationToken ct) =>
        await _patientService.SearchPatientsAsync(new Hospital.Data.Models.DTOs.SearchPatientsRequest
        {
            MinAge = min,
            MaxAge = max,
            Sex = s,
            Cnp = q?.All(char.IsDigit) == true && q.Length == 13 ? q : null,
            NamePart = q?.All(char.IsDigit) == false ? q : null
        }, ct);

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

    private static object BuildIndexRouteValues(string? searchQuery, int? minAge, int? maxAge, Sex? filterSex, bool archived, int? selectedId) =>
        new
        {
            search = searchQuery,
            minAge,
            maxAge,
            sex = filterSex,
            archived,
            selectedId
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
