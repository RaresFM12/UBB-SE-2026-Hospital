using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Hospital.Shared.Services;
using Hospital.Web.Models.Patient;
using Patient = Hospital.Shared.Models.PatientEr.Patient;
using MedicalRecord = Hospital.Shared.Models.PatientEr.MedicalRecord;
using IBillingService = Hospital.Shared.Services.IBillingService;

namespace Hospital.Web.Controllers;

[Authorize]
public class PatientsController : Controller
{
    private const string EmergencyRoomRecordSource = "ER";
    private const string MissingValueLabel = "N/A";
    private const int MissingStaffId = 0;
    private const int NoDiscountApplied = 0;

    private readonly IPatientService _patientService;
    private readonly IBillingService _billingService;

    public PatientsController(IPatientService patientService, IBillingService billingService)
    {
        _patientService = patientService;
        _billingService = billingService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var patients = await _patientService.GetPatientsAsync(cancellationToken);
        return View(patients);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, int? selectedRecordId, CancellationToken cancellationToken = default)
    {
        try
        {
            PatientProfileViewModel model = await BuildProfileAsync(id, selectedRecordId, cancellationToken);
            ApplyTemporaryDiscount(model);
            return View(model);
        }
        catch (UnauthorizedAccessException) { return RedirectToLogin(); }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index", "Admin");
        }
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

    private IActionResult RedirectToLogin() => RedirectToAction("Login", "Auth");
}
