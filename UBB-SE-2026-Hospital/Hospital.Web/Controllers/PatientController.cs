using System.Text;
using Common.Data.Entity;
using Common.Data.Entity.DTOs;
using Hospital.Web.Models.PatientProfile;
using Hospital.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Hospital.Web.Controllers;

[Authorize]
public class PatientController : Controller
{
    private readonly IPatientService patientService;
    private readonly IBillingService billingService;
    private readonly IErWorkflowService erWorkflowService;
    private readonly IAppointmentImportProvider appointmentImportProvider;

    public PatientController(
        IPatientService patientService,
        IBillingService billingService,
        IErWorkflowService erWorkflowService,
        IAppointmentImportProvider appointmentImportProvider)
    {
        this.patientService = patientService;
        this.billingService = billingService;
        this.erWorkflowService = erWorkflowService;
        this.appointmentImportProvider = appointmentImportProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var patients = await patientService.GetPatientsAsync(cancellationToken);
        return View(patients);
    }

    [HttpGet]
    public async Task<IActionResult> Details(
        int id,
        int? selectedRecordId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            PatientProfileViewModel model = await BuildProfileAsync(id, selectedRecordId, cancellationToken);
            ApplyTemporaryDiscount(model);
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index", "Admin");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Prescription(
        int patientId,
        int recordId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Prescription? prescription = await patientService.GetPrescriptionByRecordIdAsync(recordId, cancellationToken);
            if (prescription is null)
            {
                TempData["ErrorMessage"] = "This consultation does not have an associated prescription.";
                return RedirectToAction(nameof(Details), new { id = patientId, selectedRecordId = recordId });
            }

            PatientPrescriptionViewModel model = MapPrescription(prescription);
            ViewData["PatientId"] = patientId;
            ViewData["RecordId"] = recordId;
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyDiscount(
        int patientId,
        int recordId,
        int discount,
        CancellationToken cancellationToken = default)
    {
        if (discount is < 0 or > 100)
        {
            TempData["ErrorMessage"] = "Discount must be between 0 and 100.";
            return RedirectToAction(nameof(Details), new { id = patientId, selectedRecordId = recordId });
        }

        try
        {
            decimal basePrice = await billingService.ComputeBasePriceAsync(patientId, recordId, cancellationToken);
            decimal discountedPrice = await billingService.ApplyDiscountAsync(recordId, basePrice, discount, cancellationToken);

            TempData["SuccessMessage"] = $"Applied a {discount}% discount. Final price: {discountedPrice:C}.";
            TempData["TemporaryDiscount"] = discount.ToString();
            TempData["TemporaryDiscountedPrice"] = discountedPrice.ToString(System.Globalization.CultureInfo.InvariantCulture);

            return RedirectToAction(nameof(Details), new { id = patientId, selectedRecordId = recordId });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id = patientId, selectedRecordId = recordId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportRecord(
        int patientId,
        int recordId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            RecordExportDataDto exportData = await patientService.GetRecordExportDataAsync(recordId, cancellationToken);
            byte[] bytes = Encoding.UTF8.GetBytes(BuildExportText(exportData));
            string fileName = $"MedicalRecord_{exportData.Patient.FirstName}{exportData.Patient.LastName}_{exportData.Record.ConsultationDate:yyyyMMdd}.txt";
            return File(bytes, "text/plain", fileName);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id = patientId, selectedRecordId = recordId });
        }
    }

    private async Task<PatientProfileViewModel> BuildProfileAsync(
        int id,
        int? selectedRecordId,
        CancellationToken cancellationToken)
    {
        Patient patient = await patientService.GetPatientDetailsAsync(id, cancellationToken);
        MedicalHistory? history = patient.MedicalHistory;

        List<PatientRecordViewModel> records = history?.MedicalRecords?
            .OrderByDescending(r => r.ConsultationDate)
            .Select(MapRecord)
            .ToList() ?? new List<PatientRecordViewModel>();

        PatientRecordViewModel? selectedRecord = selectedRecordId.HasValue
            ? records.FirstOrDefault(r => r.Id == selectedRecordId.Value)
            : records.FirstOrDefault();

        PatientPrescriptionViewModel? prescription = null;
        decimal? basePrice = null;
        decimal? finalPrice = null;

        if (selectedRecord is not null)
        {
            basePrice = await TryComputeBasePriceAsync(patient.Id, selectedRecord.Id, cancellationToken);
            finalPrice = selectedRecord.FinalPrice > 0 ? selectedRecord.FinalPrice : basePrice;
            prescription = await TryLoadPrescriptionAsync(selectedRecord.Id, cancellationToken);
        }

        return new PatientProfileViewModel
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Dob = patient.Dob,
            Sex = patient.Sex.ToString(),
            Cnp = patient.Cnp,
            PhoneNo = FormatPhoneNumber(patient.PhoneNo),
            EmergencyContact = FormatPhoneNumber(patient.EmergencyContact),
            IsArchived = patient.IsArchived,
            IsHighRisk = await TryLoadHighRiskAsync(patient.Id, cancellationToken),
            BloodType = history?.BloodType?.ToString() ?? "N/A",
            Rh = history?.Rh?.ToString() ?? "N/A",
            ChronicConditions = history?.ChronicConditions is { Count: > 0 }
                ? string.Join(", ", history.ChronicConditions)
                : "None",
            Allergies = await patientService.GetPatientAllergiesAsync(patient.Id, cancellationToken),
            MedicalRecords = records,
            SelectedRecordId = selectedRecord?.Id,
            SelectedRecord = selectedRecord,
            SelectedPrescription = prescription,
            BasePrice = basePrice,
            FinalPrice = finalPrice
        };
    }

    private async Task<decimal?> TryComputeBasePriceAsync(
        int patientId,
        int recordId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await billingService.ComputeBasePriceAsync(patientId, recordId, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async Task<PatientPrescriptionViewModel?> TryLoadPrescriptionAsync(
        int recordId,
        CancellationToken cancellationToken)
    {
        try
        {
            Prescription? prescription = await patientService.GetPrescriptionByRecordIdAsync(recordId, cancellationToken);
            return prescription is null ? null : MapPrescription(prescription);
        }
        catch
        {
            return null;
        }
    }

    private async Task<bool> TryLoadHighRiskAsync(int patientId, CancellationToken cancellationToken)
    {
        try
        {
            return await patientService.IsHighRiskPatientAsync(patientId, cancellationToken);
        }
        catch
        {
            return false;
        }
    }

    private void ApplyTemporaryDiscount(PatientProfileViewModel model)
    {
        if (TempData["TemporaryDiscount"] is string discountText
            && int.TryParse(discountText, out int discount))
        {
            model.TemporaryDiscount = discount;
        }

        if (TempData["TemporaryDiscountedPrice"] is string priceText
            && decimal.TryParse(
                priceText,
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal discountedPrice))
        {
            model.TemporaryDiscountedPrice = discountedPrice;
            model.FinalPrice = discountedPrice;
        }
    }

    private static PatientRecordViewModel MapRecord(MedicalRecord record)
    {
        return new PatientRecordViewModel
        {
            Id = record.Id,
            ConsultationDate = record.ConsultationDate,
            SourceType = record.SourceType.ToString(),
            StaffId = record.StaffId,
            Symptoms = record.Symptoms ?? "N/A",
            Diagnosis = record.Diagnosis ?? "N/A",
            BasePrice = record.BasePrice,
            FinalPrice = record.FinalPrice,
            DiscountApplied = record.DiscountApplied
        };
    }

    private static PatientPrescriptionViewModel MapPrescription(Prescription prescription)
    {
        return new PatientPrescriptionViewModel
        {
            Id = prescription.Id,
            Date = prescription.Date,
            DoctorNotes = prescription.DoctorNotes ?? "None",
            Items = prescription.MedicationList
                .Select(item => new PatientPrescriptionItemViewModel
                {
                    MedicationName = item.MedName,
                    Quantity = item.Quantity ?? string.Empty
                })
                .ToList()
        };
    }

    private static string BuildExportText(RecordExportDataDto data)
    {
        var builder = new StringBuilder();
        _ = builder.AppendLine($"Patient: {data.Patient.FirstName} {data.Patient.LastName}");
        _ = builder.AppendLine($"CNP: {data.Patient.Cnp}");
        _ = builder.AppendLine($"Consultation Date: {data.Record.ConsultationDate:yyyy-MM-dd HH:mm}");
        _ = builder.AppendLine();
        _ = builder.AppendLine("Clinical Findings");
        _ = builder.AppendLine($"Symptoms: {data.Record.Symptoms ?? "N/A"}");
        _ = builder.AppendLine($"Diagnosis: {data.Record.Diagnosis ?? "N/A"}");
        _ = builder.AppendLine();
        _ = builder.AppendLine("Prescribed Treatment");

        if (data.Prescription is null || data.Items.Count == 0)
        {
            _ = builder.AppendLine("No prescription issued for this consultation.");
        }
        else
        {
            _ = builder.AppendLine($"Doctor Notes: {data.Prescription.DoctorNotes ?? "None"}");
            foreach (var item in data.Items)
            {
                _ = builder.AppendLine($"- {item.MedName}: {item.Quantity}");
            }
        }

        return builder.ToString();
    }

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before opening patient details.";
        return RedirectToAction("AuthenticationView", "Authentication");
    }

    private static string FormatPhoneNumber(string phone)
    {
        const int LocalPhoneLength = 10;
        const int FirstGroupStart = 1;
        const int SecondGroupStart = 4;
        const int ThirdGroupStart = 7;
        const int GroupLength = 3;

        if (string.IsNullOrWhiteSpace(phone))
        {
            return phone;
        }

        string normalized = NormalizePhone(phone);
        if (!normalized.StartsWith('0') || normalized.Length != LocalPhoneLength)
        {
            return phone;
        }

        return $"+40 {normalized.Substring(FirstGroupStart, GroupLength)} {normalized.Substring(SecondGroupStart, GroupLength)} {normalized.Substring(ThirdGroupStart, GroupLength)}";
    }

    private static string NormalizePhone(string phone)
    {
        const int CountryCodeLength = 3;

        if (string.IsNullOrWhiteSpace(phone))
        {
            return phone;
        }

        string normalized = phone.Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);
        return normalized.StartsWith("+40", StringComparison.Ordinal) ? $"0{normalized[CountryCodeLength..]}" : normalized;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportFromEr(int patientId)
    {
        try
        {
            Patient patient = await patientService.GetPatientDetailsAsync(patientId, HttpContext.RequestAborted);
            if (patient.MedicalHistory is null)
            {
                throw new InvalidOperationException("Patient medical history must be initialized before importing records.");
            }

            var existingErSourceIds = patient.MedicalHistory.MedicalRecords?
                .Where(record => record.SourceType == Common.Data.Entity.Enums.SourceType.ER)
                .Select(record => record.SourceId)
                .ToHashSet() ?? new HashSet<int>();

            Examination? candidateExam = (await erWorkflowService.GetPatientExaminationHistoryAsync(
                    patient.Cnp,
                    HttpContext.RequestAborted))
                .OrderByDescending(examination => examination.Exam_Time)
                .FirstOrDefault(examination => !existingErSourceIds.Contains(examination.Visit_ID));

            if (candidateExam is null)
            {
                throw new InvalidOperationException("No new ER examination is available to import for this patient.");
            }

            ERExaminationSummaryDto? summary = await erWorkflowService.GetExaminationSummaryAsync(
                candidateExam.Visit_ID,
                HttpContext.RequestAborted);
            if (summary is null)
            {
                throw new InvalidOperationException("Could not load the ER examination summary for import.");
            }

            var dto = new RecordDTO
            {
                ExternalRecordId = candidateExam.Visit_ID,
                Symptoms = summary.ChiefComplaint,
                TemporaryDiagnosis = string.IsNullOrWhiteSpace(summary.Notes) ? summary.Specialization : summary.Notes,
                PrescribedMeds = string.Empty,
                ConsultationDate = summary.ExamTime,
                SourceType = Common.Data.Entity.Enums.SourceType.ER,
            };

            await ProcessImportAsync(dto, patient, HttpContext.RequestAborted);
            TempData["SuccessMessage"] = "ER records imported correctly.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = patientId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportFromStaff(int patientId)
    {
        try
        {
            Patient patient = await patientService.GetPatientDetailsAsync(patientId, HttpContext.RequestAborted);
            RecordDTO dto = appointmentImportProvider.FetchRecordByPatientId(patientId);
            await ProcessImportAsync(dto, patient, HttpContext.RequestAborted);
            TempData["SuccessMessage"] = "Staff records imported correctly.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = patientId });
    }

    private async Task ProcessImportAsync(RecordDTO dto, Patient patient, CancellationToken cancellationToken)
    {
        if (patient.MedicalHistory is null)
        {
            throw new InvalidOperationException("Patient medical history must be initialized before importing records.");
        }

        int recordId = await patientService.CreateMedicalRecordAsync(
            patient.Id,
            BuildRecordFromDto(dto),
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(dto.PrescribedMeds))
        {
            await CreatePrescriptionAsync(dto.PrescribedMeds, recordId, cancellationToken);
        }
    }

    private async Task CreatePrescriptionAsync(string medsString, int recordId, CancellationToken cancellationToken)
    {
        string[] meds = medsString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var prescription = new Prescription
        {
            Date = DateTime.Now,
            DoctorNotes = "Imported from external provider",
            MedicationList = meds.Select(medication => new PrescriptionItem
            {
                MedName = medication,
                Quantity = "1",
            }).ToList(),
        };

        await patientService.CreatePrescriptionAsync(recordId, prescription);
    }

    private static MedicalRecord BuildRecordFromDto(RecordDTO dto)
    {
        return new MedicalRecord
        {
            SourceType = dto.SourceType,
            SourceId = dto.ExternalRecordId,
            StaffId = 1,
            Symptoms = dto.Symptoms,
            Diagnosis = dto.TemporaryDiagnosis,
            ConsultationDate = dto.ConsultationDate,
            BasePrice = 0,
            FinalPrice = 0,
            PoliceNotified = false,
        };
    }
}
