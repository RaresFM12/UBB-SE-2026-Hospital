using Hospital.Data.Models;
using Hospital.Services;
using Hospital.Shared.Services;
using Hospital.Web.Models.BloodCompatibility;
using Hospital.Web.Models.Patients;
using Hospital.Web.Models.Transplant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DbPatient = Hospital.Data.Models.Patient;
using IBloodCompatibilityService = Hospital.Services.IBloodCompatibilityService;
using ITransplantService = Hospital.Services.ITransplantService;
using SharedPatient = Hospital.Data.Models.Patient;

namespace Hospital.Web.Controllers;

[Authorize]
public class OrganDonorController : Controller
{
    private const int NoCompatibilityScore = 0;
    private const int ExactBloodRhMatchScore = 50;
    private const int PartialBloodRhMatchScore = 25;
    private const int MaxAgeScore = 30;
    private const int AgeScoreStepYears = 5;
    private const int SameSexScore = 20;
    private const int DifferentSexScore = 10;

    private readonly Hospital.Services.IBloodCompatibilityService bloodCompatibilityService;
    private readonly ITransplantService transplantService;
    private readonly IPatientService patientService;

    public OrganDonorController(
        IBloodCompatibilityService bloodCompatibilityService,
        ITransplantService transplantService,
        IPatientService patientService)
    {
        this.bloodCompatibilityService = bloodCompatibilityService;
        this.transplantService = transplantService;
        this.patientService = patientService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? recipientSearch, CancellationToken cancellationToken)
    {
        return View(new OrganDonorViewModel
        {
            RecipientSearchQuery = recipientSearch,
            RecipientPatients = await LoadRecipientPatientsAsync(recipientSearch, cancellationToken)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FindDonors(int patientId, string organ, CancellationToken cancellationToken)
    {
        DbPatient? patient = await patientService.GetByIdAsync(patientId, cancellationToken);
        if (patient is null)
        {
            TempData["ErrorMessage"] = "Patient not found.";
            return RedirectToAction(nameof(Index));
        }

        if (patient.IsDeceased)
        {
            TempData["ErrorMessage"] = "Please select a living recipient patient.";
            return RedirectToAction(nameof(Index), new { recipientSearch = patient.Cnp });
        }

        if (string.IsNullOrWhiteSpace(organ))
        {
            TempData["ErrorMessage"] = "Please choose an organ first.";
            return RedirectToAction(nameof(Index), new { recipientSearch = patient.Cnp });
        }

        SharedPatient recipientDetails = await patientService.GetPatientDetailsAsync(patient.PatientId, cancellationToken);

        var model = new OrganDonorViewModel
        {
            RecipientSearchQuery = patient.Cnp,
            RecipientPatients = await LoadRecipientPatientsAsync(patient.Cnp, cancellationToken),
            PatientId = patient.PatientId,
            PatientName = patient.FullName,
            SelectedOrgan = organ
        };

        try
        {
            var pendingTransplant = (await transplantService.GetByPatientIdAsync(patient.PatientId))
                .Where(transplant => transplant.Status == Hospital.Data.Models.TransplantStatus.Pending
                    && string.Equals(transplant.OrganType, organ, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(transplant => transplant.RequestDate)
                .FirstOrDefault();

            if (pendingTransplant is null)
            {
                await transplantService.CreateWaitlistRequestAsync(patient.PatientId, organ);

                pendingTransplant = (await transplantService.GetByPatientIdAsync(patient.PatientId))
                    .Where(transplant => transplant.Status == Hospital.Data.Models.TransplantStatus.Pending
                        && string.Equals(transplant.OrganType, organ, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(transplant => transplant.RequestDate)
                    .FirstOrDefault();

                if (pendingTransplant is null)
                {
                    model.ErrorMessage = "The transplant request could not be prepared.";
                    return View("Index", model);
                }
            }

            model.PendingTransplantId = pendingTransplant.TransplantId;

            var topDonors = await bloodCompatibilityService.GetTopCompatibleDonorsAsync(patient.PatientId);
            model.TopDonors = topDonors.Select(donor => new DonorMatchViewModel
            {
                PatientId = donor.PatientId,
                FirstName = donor.FirstName,
                LastName = donor.LastName,
                Cnp = donor.Cnp,
                BloodType = donor.MedicalHistory?.BloodType?.ToString() ?? "Unknown",
                RhFactor = donor.MedicalHistory?.Rh?.ToString() ?? "Unknown",
                Score = CalculateDisplayScore(donor, recipientDetails)
            }).ToList();

            if (model.TopDonors.Count == 0)
            {
                model.StatusMessage = $"No compatible deceased donors were found for {organ}.";
            }
        }
        catch (InvalidOperationException ex)
        {
            model.ErrorMessage = ex.Message;
        }

        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int patientId, int transplantId, int donorId, int compatibilityScore, CancellationToken cancellationToken)
    {
        try
        {
            await transplantService.AssignDonorAsync(transplantId, donorId, compatibilityScore);
            TempData["SuccessMessage"] = "Organ donor assignment confirmed successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        DbPatient? patient = await patientService.GetByIdAsync(patientId, cancellationToken);
        return RedirectToAction("Index", "Patients", new
        {
            archived = patient?.IsArchived ?? false,
            selectedId = patientId
        });
    }

    private async Task<List<PatientListItemViewModel>> LoadRecipientPatientsAsync(
        string? search,
        CancellationToken cancellationToken)
    {
        List<DbPatient> patients = await patientService.SearchPatientsAsync(new SearchPatientsRequest
        {
            Cnp = search?.All(char.IsDigit) == true && search.Length == 13 ? search : null,
            NamePart = search?.All(char.IsDigit) == false ? search : null
        }, cancellationToken);

        return patients
            .Where(patient => !patient.IsDeceased)
            .OrderBy(patient => patient.LastName)
            .ThenBy(patient => patient.FirstName)
            .Select(patient => new PatientListItemViewModel
            {
                Id = patient.PatientId,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Cnp = patient.Cnp,
                Dob = patient.DateOfBirth,
                Sex = patient.Sex.ToString(),
                PhoneNo = patient.PhoneNumber,
                EmergencyContact = patient.EmergencyContact,
                IsArchived = patient.IsArchived,
                IsDeceased = patient.IsDeceased,
                IsDonor = patient.IsDonor
            })
            .ToList();
    }

    private static int CalculateDisplayScore(SharedPatient donor, SharedPatient recipient)
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

        total += donor.Sex == recipient.Sex
            ? SameSexScore
            : DifferentSexScore;

        return total;
    }
}
