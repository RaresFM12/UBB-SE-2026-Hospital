using Hospital.Shared.Models.PatientEr;
using Hospital.Shared.Services;
using Hospital.Services.PatientEr;
using Hospital.Web.Models.BloodCompatibility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Patient = Hospital.Shared.Models.PatientEr.Patient;

namespace Hospital.Web.Controllers;

[Authorize]
public class BloodCompatibilityController : Controller
{
    private const int NoCompatibilityScore = 0;
    private const int ExactBloodRhMatchScore = 50;
    private const int PartialBloodRhMatchScore = 25;
    private const int MaxAgeScore = 30;
    private const int AgeScoreStepYears = 5;
    private const int SameSexScore = 20;
    private const int DifferentSexScore = 10;

    private readonly IBloodCompatibilityService _bloodCompatibilityService;
    private readonly IPatientService _patientService;

    public BloodCompatibilityController(
        IBloodCompatibilityService bloodCompatibilityService,
        IPatientService patientService)
    {
        _bloodCompatibilityService = bloodCompatibilityService;
        _patientService = patientService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Donors(int patientId)
    {
        Patient? patient = await _patientService.GetPatientDetailsAsync(patientId, default);

        if (patient is null)
        {
            TempData["ErrorMessage"] = "Patient not found.";
            return RedirectToAction("Index", "Admin");
        }

        var model = new BloodDonorsViewModel
        {
            PatientId = patientId,
            PatientName = $"{patient.FirstName} {patient.LastName}",
        };

        if (patient.MedicalHistory?.BloodType is null || patient.MedicalHistory?.Rh is null)
        {
            model.StatusMessage = "The selected patient needs a blood type and Rh factor in their medical history first.";
            return View(model);
        }

        List<Patient> topDonors;
        try
        {
            topDonors = await _bloodCompatibilityService.GetTopCompatibleDonorsAsync(patientId);
        }
        catch (InvalidOperationException ex)
        {
            model.StatusMessage = ex.Message;
            return View(model);
        }

        model.Donors = topDonors.Select(donor => new DonorMatchViewModel
        {
            FirstName = donor.FirstName,
            LastName = donor.LastName,
            Cnp = donor.Cnp,
            BloodType = donor.MedicalHistory?.BloodType?.ToString() ?? "Unknown",
            RhFactor = donor.MedicalHistory?.Rh?.ToString() ?? "Unknown",
            Score = CalculateScore(donor, patient),
        }).ToList();

        if (model.Donors.Count == 0)
        {
            model.StatusMessage = "No compatible blood donors were found for this patient.";
        }

        return View(model);
    }

    private static int CalculateScore(Patient donor, Patient recipient)
    {
        if (donor.MedicalHistory is null || recipient.MedicalHistory is null)
        {
            return NoCompatibilityScore;
        }

        int total = donor.MedicalHistory.BloodType == recipient.MedicalHistory.BloodType
                    && donor.MedicalHistory.Rh == recipient.MedicalHistory.Rh
            ? ExactBloodRhMatchScore
            : PartialBloodRhMatchScore;

        int ageGap = Math.Abs(donor.DateOfBirth.Year - recipient.DateOfBirth.Year);
        total += Math.Max(NoCompatibilityScore, MaxAgeScore - (ageGap / AgeScoreStepYears * AgeScoreStepYears));
        total += donor.Sex == recipient.Sex ? SameSexScore : DifferentSexScore;

        return total;
    }
}
