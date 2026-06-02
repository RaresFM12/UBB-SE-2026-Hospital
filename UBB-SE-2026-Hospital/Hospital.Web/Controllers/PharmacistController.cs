using Hospital.Data.Models;
using Hospital.Web.Models.Pharmacist;
using Hospital.Web.Models.Prescription;
using Hospital.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize]
public class PharmacistController : Controller
{
    private const int MissingPatientId = 0;

    private readonly IPrescriptionApiClient prescriptionApiClient;
    private readonly IAddictDetectionApiClient addictDetectionApiClient;

    public PharmacistController(
        IPrescriptionApiClient prescriptionApiClient,
        IAddictDetectionApiClient addictDetectionApiClient)
    {
        this.prescriptionApiClient = prescriptionApiClient;
        this.addictDetectionApiClient = addictDetectionApiClient;
    }

    [HttpGet]
    public IActionResult Index() => RedirectToAction("Feed", "Prescription");

    [HttpGet]
    public IActionResult Prescriptions() => RedirectToAction("Feed", "Prescription");

    [HttpGet]
    public async Task<IActionResult> PrescriptionDetail(int id, int? returnPatientId = null, int? returnRecordId = null)
    {
        Prescription? prescription;
        try
        {
            prescription = await prescriptionApiClient.GetPrescriptionDetailsAsync(id, HttpContext.RequestAborted);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Prescriptions));
        }

        if (prescription is null)
        {
            TempData["ErrorMessage"] = $"Prescription #{id} not found.";
            return RedirectToAction(nameof(Prescriptions));
        }

        var model = new PrescriptionDetailViewModel
        {
            Id = prescription.PrescriptionId,
            PatientName = prescription.PatientName,
            DoctorName = prescription.DoctorName,
            Date = prescription.Date,
            DoctorNotes = prescription.DoctorNotes ?? "No notes provided",
            ReturnPatientId = returnPatientId,
            ReturnRecordId = returnRecordId,
            Medications = (prescription.MedicationList ?? new List<PrescriptionItem>())
                .Select(medication => new PrescriptionItemViewModel
                {
                    MedName = medication.MedicationName,
                    Quantity = medication.Quantity
                })
                .ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Addicts()
    {
        List<Patient> candidates;
        try
        {
            candidates = await addictDetectionApiClient.GetCandidatesAsync(HttpContext.RequestAborted);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            candidates = new List<Patient>();
        }

        var model = new AddictListViewModel
        {
            Candidates = candidates
                .Where(patient => !patient.IsPoliceNotified)
                .Select(patient => new AddictCandidateViewModel
                {
                    Id = patient.PatientId,
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    IsPoliceNotified = patient.IsPoliceNotified
                })
                .ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BuildPoliceReport(int patientId)
    {
        try
        {
            string report = await addictDetectionApiClient.BuildPoliceReportAsync(patientId, HttpContext.RequestAborted);
            TempData["PoliceReportText"] = report;
            TempData["PoliceReportPatientId"] = patientId;
            return RedirectToAction(nameof(PoliceAlert));
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Addicts));
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Addicts));
        }
    }

    [HttpGet]
    public IActionResult PoliceAlert()
    {
        string? reportText = TempData["PoliceReportText"] as string;
        if (string.IsNullOrEmpty(reportText))
        {
            return RedirectToAction(nameof(Addicts));
        }

        int patientId = TempData["PoliceReportPatientId"] as int? ?? MissingPatientId;
        bool alreadySent = TempData["PoliceReportSent"] as bool? ?? false;

        var model = new PoliceAlertViewModel
        {
            ReportText = reportText,
            PatientId = patientId,
            AlreadySent = alreadySent
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPoliceReport(int patientId, string reportText)
    {
        try
        {
            await addictDetectionApiClient.MarkPoliceNotifiedAsync(patientId, HttpContext.RequestAborted);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            TempData["PoliceReportText"] = reportText;
            TempData["PoliceReportPatientId"] = patientId;
            return RedirectToAction(nameof(PoliceAlert));
        }

        TempData["PoliceReportText"] = reportText;
        TempData["PoliceReportPatientId"] = patientId;
        TempData["PoliceReportSent"] = true;
        return RedirectToAction(nameof(PoliceAlert));
    }
}
