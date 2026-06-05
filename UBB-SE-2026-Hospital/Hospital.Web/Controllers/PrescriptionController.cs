using Hospital.Shared.Proxies;
using System.Security.Cryptography;
using Hospital.Data.Models;
using Hospital.Data.Models;
using Hospital.Web.Models.Prescription;
using Hospital.Web.Models; 
using Hospital.Shared.Services; 
using Hospital.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPrescriptionService = Hospital.Services.IPrescriptionService;

namespace Hospital.Web.Controllers;

[Authorize]
public class PrescriptionController : Controller
{
    private const int FirstPage = 1;
    private const int EmptyCollectionCount = 0;
    private const int EmptyTextLength = 0;
    private const int PageIndexAdjustment = 1;

    private readonly IPrescriptionService prescriptionService;
    private readonly IAdminService adminService;
    private readonly IMedicalEvaluationService evaluationService;

    private const int PageSize = 9;
    private static readonly char[] MedicineSeparators = new[] { ',', ';', '\n', '\r' };

    public PrescriptionController(
        IPrescriptionService prescriptionService,
        IAdminService adminService,
        IMedicalEvaluationService evaluationService)
    {
        this.prescriptionService = prescriptionService;
        this.adminService = adminService;
        this.evaluationService = evaluationService;
    }

    [HttpGet]
    [Authorize(Roles = "Pharmacist,Admin")]
    public IActionResult Resolve()
    {
        return View("Index", new ResolvePrescriptionViewModel());
    }

    [HttpPost]
    [Authorize(Roles = "Pharmacist,Admin")]
    [ValidateAntiForgeryToken]
    public IActionResult Resolve(ResolvePrescriptionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        var warnings = BuildHighRiskWarnings(model.PrescriptionId).GetAwaiter().GetResult();

        var populatedModel = new ResolvePrescriptionViewModel
        {
            PrescriptionId = model.PrescriptionId,
            HasResult = true,
            ResolvedRows = new List<ResolvedItemRow>(),
            HighRiskWarnings = warnings,
        };

        return View("Index", populatedModel);
    }

    [HttpGet]
    public async Task<IActionResult> Feed(
        string? searchIdText,
        string? searchName,
        string? searchMedication,
        DateTime? dateFrom,
        DateTime? dateTo,
        int page = FirstPage,
        int? returnPatientId = null,
        CancellationToken cancellationToken = default)
    {
        var model = new PrescriptionFeedViewModel
        {
            SearchIdText = searchIdText,
            SearchName = searchName,
            SearchMedication = searchMedication,
            DateFrom = dateFrom,
            DateTo = dateTo,
            CurrentPage = page,
            ReturnPatientId = returnPatientId
        };

        try
        {
            List<Prescription> prescriptions;

            bool hasFilter =
                !string.IsNullOrWhiteSpace(searchIdText) ||
                !string.IsNullOrWhiteSpace(searchName) ||
                !string.IsNullOrWhiteSpace(searchMedication) ||
                dateFrom.HasValue ||
                dateTo.HasValue;

            if (hasFilter)
            {
                var filter = new PrescriptionFilter
                {
                    PrescriptionId = TryParseInt(searchIdText),
                    PatientName = searchName,
                    MedicationName = searchMedication,
                    DateFrom = dateFrom,
                    DateTo = dateTo
                };

                prescriptions = (await prescriptionService.ApplyFilterAsync(filter))
                    .Skip((page - PageIndexAdjustment) * PageSize)
                    .Take(PageSize)
                    .ToList();
            }
            else
            {
                prescriptions = await prescriptionService.GetLatestPrescriptionsAsync(PageSize, page);
            }

            if (prescriptions.Count == EmptyCollectionCount)
            {
                model.InfoMessage = "No prescriptions found.";
            }

            model.Prescriptions = prescriptions.Select(p => new PrescriptionCardViewModel
            {
                Id = p.PrescriptionId,
                PatientName = p.PatientName ?? string.Empty,
                DoctorName = ResolveDoctorName(
                    p.MedicalRecord?.StaffMember is { } s
                        ? $"{s.FirstName} {s.LastName}"
                        : p.DoctorName),
                DoctorNotes = p.DoctorNotes ?? string.Empty,
                Date = p.Date,
                Items = p.MedicationList?.Select(i => new PrescriptionItemViewModel
                {
                    MedName = i.MedicationName,
                    Quantity = i.Quantity
                }).ToList() ?? new List<PrescriptionItemViewModel>()
            }).ToList();
        }
        catch (Exception ex)
        {
            model.InfoMessage = "Error loading prescriptions: " + ex.Message;
        }

        return View(model);
    }

    private async Task<IReadOnlyList<ResolvedItemRow>> BuildResolvedRows(Dictionary<int, int> resolved)
    {
        var rows = new List<ResolvedItemRow>(resolved.Count);
        foreach (var resolvedEntry in resolved)
        {
            int itemId = resolvedEntry.Key;
            int boxQuantity = resolvedEntry.Value;
            Item item = await adminService.GetItemByIdAsync(itemId);
            if (item == null) continue;

            rows.Add(new ResolvedItemRow
            {
                ItemId = item.Id,
                ItemName = item.Name,
                Producer = item.Producer,
                PillsPerBox = item.NumberOfPills,
                BoxQuantity = boxQuantity,
                UnitPrice = item.Price,
                TotalPrice = item.Price * boxQuantity,
            });
        }
        return rows;
    }

    private async Task<IReadOnlyList<string>> BuildHighRiskWarnings(string prescriptionId)
    {
        if (!int.TryParse((prescriptionId ?? string.Empty).Trim(), out int evaluationId))
        {
            return new List<string>();
        }

        MedicalEvaluation? evaluation = await evaluationService.GetEvaluationByIdAsync(evaluationId);

        if (evaluation == null || string.IsNullOrWhiteSpace(evaluation.MedicationsList))
        {
            return new List<string>();
        }

        var highRiskReference = await adminService.GetHighRiskMedicinesAsync();
        var warnings = new List<string>();

        foreach (var medicineName in ParseMedicineNames(evaluation.MedicationsList))
        {
            foreach (var highRiskEntry in highRiskReference)
            {
                if (string.Equals(highRiskEntry.MedicineName, medicineName, StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(highRiskEntry.WarningMessage))
                    {
                        warnings.Add($"{highRiskEntry.MedicineName}: {highRiskEntry.WarningMessage}");
                    }
                    break;
                }
            }
        }
        return warnings;
    }

    private static IEnumerable<string> ParseMedicineNames(string medicationsList)
    {
        foreach (var part in medicationsList.Split(MedicineSeparators, StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = part.Trim();
            if (trimmed.Length > EmptyTextLength) yield return trimmed;
        }
    }

    private static string ResolveDoctorName(string? doctorName)
    {
        if (!string.IsNullOrEmpty(doctorName) && !doctorName.Contains("Unknown", StringComparison.OrdinalIgnoreCase))
            return doctorName;

        var fakeDoctors = MockDoctorProvider.FakeDoctors;
        int index = RandomNumberGenerator.GetInt32(fakeDoctors.Count);
        var doc = fakeDoctors[index];
        return $"Dr. {doc.FirstName} {doc.LastName}";
    }

    private static int? TryParseInt(string? value) =>
        int.TryParse(value, out int result) ? result : null;
}

