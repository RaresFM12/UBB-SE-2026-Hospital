using System.Globalization;
using System.Text;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
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
    private const int AddictMinimumMedications = 2;
    private const int AddictPrescriptionFetchLimit = 1000;

    private static readonly HashSet<int> NotifiedCandidateIds = new();

    private readonly IPrescriptionApiClient prescriptionApiClient;
    private readonly IAddictDetectionApiClient addictDetectionApiClient;
    private readonly IPrescriptionRepository prescriptionRepository;

    public PharmacistController(
        IPrescriptionApiClient prescriptionApiClient,
        IAddictDetectionApiClient addictDetectionApiClient,
        IPrescriptionRepository prescriptionRepository)
    {
        this.prescriptionApiClient = prescriptionApiClient;
        this.addictDetectionApiClient = addictDetectionApiClient;
        this.prescriptionRepository = prescriptionRepository;
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
        List<Prescription> prescriptions = await prescriptionRepository
            .GetTopNAsync(AddictPrescriptionFetchLimit, 1);

        var candidates = prescriptions
            .Where(p => p.MedicationList != null && p.MedicationList.Count >= AddictMinimumMedications)
            .Where(p => !NotifiedCandidateIds.Contains(p.PrescriptionId))
            .Select(p => new AddictCandidateViewModel
            {
                Id = p.PrescriptionId,
                FirstName = string.IsNullOrWhiteSpace(p.PatientName)
                    ? $"Patient on Prescription #{p.PrescriptionId}"
                    : p.PatientName,
                LastName = string.Empty,
                IsPoliceNotified = false
            })
            .ToList();

        var model = new AddictListViewModel { Candidates = candidates };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BuildPoliceReport(int patientId)
    {
        var filter = new Hospital.Data.Models.DTOs.PrescriptionFilter { PrescriptionId = patientId };
        var matches = await prescriptionRepository.GetFilteredAsync(filter);
        Prescription? prescription = matches.FirstOrDefault();

        if (prescription is null)
        {
            TempData["ErrorMessage"] = $"Prescription #{patientId} not found.";
            return RedirectToAction(nameof(Addicts));
        }

        string report = BuildPoliceReportText(prescription);
        TempData["PoliceReportText"] = report;
        TempData["PoliceReportPatientId"] = patientId;
        return RedirectToAction(nameof(PoliceAlert));
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
    public IActionResult ConfirmPoliceReport(int patientId, string reportText)
    {
        NotifiedCandidateIds.Add(patientId);

        TempData["PoliceReportText"] = reportText;
        TempData["PoliceReportPatientId"] = patientId;
        TempData["PoliceReportSent"] = true;
        return RedirectToAction(nameof(PoliceAlert));
    }

    private static string BuildPoliceReportText(Prescription prescription)
    {
        const string reportHeader =
            "==================================================\n" +
            "           LAW ENFORCEMENT ALERT REPORT           \n" +
            "==================================================";
        const string reportFooter =
            "--------------------------------------------------\n" +
            "SUSPICIOUS ACTIVITY: SUSPECTED DRUG SHOPPING BEHAVIOR\n" +
            "CRITERIA MET: PRESCRIPTION CONTAINS 2 OR MORE MEDICATIONS\n" +
            "--- SUPPORTING EVIDENCE (PRESCRIPTION) ---";
        const string reportPharmacistFooter =
            "==================================================\n" +
            "ACTION REQUIRED: AWAITING PHARMACIST CONFIRMATION.";

        string patientLabel = string.IsNullOrWhiteSpace(prescription.PatientName)
            ? $"Patient on Prescription #{prescription.PrescriptionId}"
            : prescription.PatientName;

        string doctorName = prescription.MedicalRecord?.StaffMember?.FullName
            ?? (string.IsNullOrWhiteSpace(prescription.DoctorName) ? "Unknown" : prescription.DoctorName);

        string medications = prescription.MedicationList is { Count: > 0 }
            ? string.Join(", ", prescription.MedicationList.Select(m => m.MedicationName))
            : "Unknown";

        var builder = new StringBuilder();
        _ = builder.AppendLine(reportHeader)
            .AppendLine(CultureInfo.InvariantCulture, $"DATE GENERATED: {DateTime.Now:yyyy-MM-dd HH:mm}")
            .AppendLine(CultureInfo.InvariantCulture, $"SUBJECT: {patientLabel}")
            .AppendLine(CultureInfo.InvariantCulture, $"DOCTOR: {doctorName}")
            .AppendLine(reportFooter)
            .AppendLine(CultureInfo.InvariantCulture, $"[1] Prescription ID: {prescription.PrescriptionId} | Date: {prescription.Date:yyyy-MM-dd}")
            .AppendLine(CultureInfo.InvariantCulture, $"    Dispensed Drugs ({prescription.MedicationList?.Count ?? 0}): {medications}")
            .AppendLine(string.Empty)
            .AppendLine(reportPharmacistFooter);

        return builder.ToString();
    }
}
