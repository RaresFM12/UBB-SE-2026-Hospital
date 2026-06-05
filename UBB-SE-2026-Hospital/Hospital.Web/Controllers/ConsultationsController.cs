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
    private readonly IPatientService patientService;
    private readonly IBillingService billingService;

    public ConsultationController(IPatientService patientService, IBillingService billingService)
    {
        this.patientService = patientService;
        this.billingService = billingService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
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
            .FirstOrDefault(r => r.RecordId == recordId);

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
