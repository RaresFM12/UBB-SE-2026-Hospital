using Common.Data.Entity;
using Hospital.Web.Models.BloodCompatibility;
using Hospital.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize]
public class BloodCompatibilityController : Controller
{
    private readonly IBloodCompatibilityService bloodCompatibilityService;
    private readonly IPatientService patientService;

    public BloodCompatibilityController(
        IBloodCompatibilityService bloodCompatibilityService,
        IPatientService patientService)
    {
        this.bloodCompatibilityService = bloodCompatibilityService;
        this.patientService = patientService;
    }

    [HttpGet]
    public async Task<IActionResult> Donors(int patientId)
    {
        Patient? patient;
        try
        {
            patient = await patientService.GetPatientDetailsAsync(patientId);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index", "Admin");
        }

        if (patient is null)
        {
            TempData["ErrorMessage"] = "Patient not found.";
            return RedirectToAction("Index", "Admin");
        }

        var model = new BloodDonorsViewModel
        {
            PatientId = patientId,
            PatientName = patient.FullName,
        };

        if (patient.MedicalHistory?.BloodType is null || patient.MedicalHistory?.Rh is null)
        {
            model.StatusMessage =
                "The selected patient needs a blood type and Rh factor in their medical history first.";
            return View(model);
        }

        List<Patient> topDonors;
        try
        {
            topDonors = await bloodCompatibilityService
                .GetTopCompatibleDonorsAsync(patientId);
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
            return 0;
        }

        int total = donor.MedicalHistory.BloodType == recipient.MedicalHistory.BloodType
                    && donor.MedicalHistory.Rh == recipient.MedicalHistory.Rh
            ? 50
            : 25;

        int ageGap = Math.Abs(donor.Dob.Year - recipient.Dob.Year);
        total += Math.Max(0, 30 - (ageGap / 5 * 5));
        total += donor.Sex == recipient.Sex ? 20 : 10;

        return total;
    }
}