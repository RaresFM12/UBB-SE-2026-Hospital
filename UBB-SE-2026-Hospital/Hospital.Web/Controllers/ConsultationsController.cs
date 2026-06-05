using Hospital.Shared.Proxies;
using Hospital.Web.Models.Consultations;
using Hospital.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IBillingService = Hospital.Shared.Services.IBillingService;
using PatientErMedicalRecord = Hospital.Data.Models.MedicalRecord;
using PatientErPatient = Hospital.Data.Models.Patient;
using PatientErPrescription = Hospital.Data.Models.Prescription;

namespace Hospital.Web.Controllers;

[Authorize]
public class ConsultationController : Controller
{
    private readonly IPatientApiClient patientService;
    private readonly IBillingApiClient billingService;

    public ConsultationController(IPatientApiClient patientService, IBillingApiClient billingService)
    {
        this.patientService = patientService;
        this.billingService = billingService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? patientId)
    {
        IReadOnlyList<PatientErPatient> allPatients = await patientService.GetAllPatients();

        var model = new ConsultationsIndexViewModel
        {
            SelectedPatientId = patientId,
            Patients = allPatients
                .OrderBy(patient => patient.LastName)
                .ThenBy(patient => patient.FirstName)
                .Select(patient => new PatientOption
                {
                    Id = patient.PatientId,
                    FullName = $"{patient.FirstName} {patient.LastName}".Trim(),
                    Cnp = patient.Cnp ?? string.Empty
                })
                .ToList()
        };

        if (patientId.HasValue)
        {
            try
            {
                PatientErPatient selected = await patientService.GetPatientDetailsAsync(patientId.Value);
                model.SelectedPatientName = $"{selected.FirstName} {selected.LastName}".Trim();

                List<PatientErMedicalRecord> records = selected.MedicalHistory?.MedicalRecords?.ToList()
                    ?? new List<PatientErMedicalRecord>();

                model.Records = records
                    .OrderByDescending(record => record.ConsultationDate)
                    .Select(record => new RecordOption
                    {
                        RecordId = record.RecordId,
                        ConsultationDate = record.ConsultationDate,
                        Diagnosis = record.Diagnosis ?? "N/A",
                        SourceType = record.SourceType.ToString()
                    })
                    .ToList();
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int patientId, int recordId)
    {
        PatientErPatient patient;
        try
        {
            patient = await patientService.GetPatientDetailsAsync(patientId);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index", "Admin");
        }

        PatientErMedicalRecord? record = patient.MedicalHistory?.MedicalRecords?
            .FirstOrDefault(record => record.RecordId == recordId);

        if (record is null)
        {
            TempData["ErrorMessage"] = "Consultation record not found.";
            return RedirectToAction("Details", "Admin", new { id = patientId });
        }

        decimal basePrice;
        try
        {
            basePrice = await billingService.ComputeBasePriceAsync(patientId, recordId);
        }
        catch (InvalidOperationException)
        {
            basePrice = record.BasePrice;
        }

        int? discountApplied = record.DiscountApplied;
        decimal finalPrice = record.DiscountApplied.HasValue ? record.FinalPrice : basePrice;

        int? prescriptionId = null;
        try
        {
            PatientErPrescription? prescription = await patientService.GetPrescriptionByRecordIdAsync(recordId);
            prescriptionId = prescription?.PrescriptionId;
        }
        catch (InvalidOperationException)
        {
            prescriptionId = null;
        }

        var model = new ConsultationDetailsViewModel
        {
            RecordId = record.RecordId,         
            PatientId = patient.PatientId,       
            PatientFirstName = patient.FirstName,
            PatientLastName = patient.LastName,
            SourceType = record.SourceType.ToString(),
            StaffId = record.StaffMember?.StaffId ?? 0, 
            ConsultationDate = record.ConsultationDate, 
            Symptoms = record.Symptoms ?? "N/A",        
            Diagnosis = record.Diagnosis ?? "N/A",      
            BasePrice = basePrice,
            FinalPrice = finalPrice,
            DiscountApplied = record.DiscountApplied,
            PrescriptionId = prescriptionId,
            IsArchived = patient.IsArchived
        };

        return View(model);
    }
}

