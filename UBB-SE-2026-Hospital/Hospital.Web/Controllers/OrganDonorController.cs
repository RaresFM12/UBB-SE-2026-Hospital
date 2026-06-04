using Hospital.Data.Models.DTOs;
using Hospital.Services.PatientEr;
using Hospital.Shared.Services;
using Hospital.Web.Models.Patients;
using Hospital.Web.Models.Transplant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DbPatient = Hospital.Data.Models.Patient;
using TransplantMatch = Hospital.Data.Models.TransplantMatch;

namespace Hospital.Web.Controllers;

[Authorize]
public class OrganDonorController : Controller
{
    private const int NoMatchesCount = 0;

    private readonly ITransplantService transplantService;
    private readonly IPatientService patientService;

    public OrganDonorController(
        ITransplantService transplantService,
        IPatientService patientService)
    {
        this.transplantService = transplantService;
        this.patientService = patientService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? recipientSearch, string? donorSearch, CancellationToken cancellationToken)
    {
        return View(new OrganDonorViewModel
        {
            RecipientSearchQuery = recipientSearch,
            DonorSearchQuery = donorSearch,
            RecipientPatients = await LoadCandidatePatientsAsync(recipientSearch, includeDeceased: false, cancellationToken),
            DonorPatients = await LoadCandidatePatientsAsync(donorSearch, includeDeceased: true, cancellationToken)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRequest(int patientId, string organ, CancellationToken cancellationToken)
    {
        DbPatient? patient = await patientService.GetByIdAsync(patientId, cancellationToken);
        if (patient is null)
        {
            TempData["ErrorMessage"] = "Patient not found.";
            return RedirectToAction(nameof(Index));
        }

        if (patient.IsDeceased)
        {
            TempData["ErrorMessage"] = "Only living patients can receive a transplant request.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(organ))
        {
            TempData["ErrorMessage"] = "Please choose an organ for the transplant request.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await transplantService.CreateWaitlistRequestAsync(patientId, organ);
            TempData["SuccessMessage"] = "The patient was added to the transplant waitlist.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { recipientSearch = patient.Cnp });
    }

    [HttpGet]
    public async Task<IActionResult> Assign(int patientId, string? organ, CancellationToken cancellationToken)
    {
        DbPatient? patient = await patientService.GetByIdAsync(patientId, cancellationToken);
        if (patient is null)
        {
            TempData["ErrorMessage"] = "Patient not found.";
            return RedirectToAction(nameof(Index));
        }

        if (!patient.IsDeceased)
        {
            TempData["ErrorMessage"] = "Patient must be marked as deceased before organ donor assignment.";
            return RedirectToAction(nameof(Index));
        }

        string? autoRegistrationMessage = null;
        if (!patient.IsDonor)
        {
            await patientService.UpdatePatientAsync(new DbPatient
            {
                PatientId = patient.PatientId,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Cnp = patient.Cnp,
                DateOfBirth = patient.DateOfBirth,
                DateOfDeath = patient.DateOfDeath,
                Sex = patient.Sex,
                PhoneNumber = patient.PhoneNumber,
                EmergencyContact = patient.EmergencyContact,
                IsArchived = patient.IsArchived,
                IsDonor = true,
                Transferred = patient.Transferred
            }, cancellationToken);

            patient.IsDonor = true;
            autoRegistrationMessage = "Patient was registered as an organ donor.";
        }

        var model = new OrganDonorViewModel
        {
            PatientId = patientId,
            PatientName = patient.FullName,
            IsDeceased = patient.IsDeceased,
            IsDonorRegistered = patient.IsDonor,
            SelectedOrgan = organ,
            StatusMessage = autoRegistrationMessage
        };

        if (!string.IsNullOrWhiteSpace(organ))
        {
            try
            {
                List<TransplantMatch> matches = await transplantService.GetTopMatchesAsDisplayModelsAsync(patientId, organ);

                model.TopMatches = matches.Select(match => new TransplantMatchViewModel
                {
                    TransplantId = match.Transplant.TransplantId,
                    ReceiverName = match.ReceiverName,
                    BloodType = match.BloodType,
                    CompatibilityScore = match.CompatibilityScore,
                    WaitingDays = match.WaitingDays,
                }).ToList();

                if (model.TopMatches.Count == NoMatchesCount)
                    model.StatusMessage = $"No compatible recipients found for {organ}.";
            }
            catch (InvalidOperationException ex)
            {
                model.ErrorMessage = ex.Message;
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int patientId, int transplantId, float compatibilityScore)
    {
        try
        {
            await transplantService.AssignDonorAsync(transplantId, patientId, compatibilityScore);
            TempData["SuccessMessage"] = "Organ donor assignment confirmed successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction("Index", "Patients", new { archived = true, selectedId = patientId });
    }

    private async Task<List<PatientListItemViewModel>> LoadCandidatePatientsAsync(
        string? search,
        bool includeDeceased,
        CancellationToken cancellationToken)
    {
        List<DbPatient> patients = await patientService.SearchPatientsAsync(new SearchPatientsRequest
        {
            Cnp = search?.All(char.IsDigit) == true && search.Length == 13 ? search : null,
            NamePart = search?.All(char.IsDigit) == false ? search : null
        }, cancellationToken);

        return patients
            .Where(patient => patient.IsDeceased == includeDeceased)
            .OrderByDescending(patient => patient.IsDeceased)
            .ThenByDescending(patient => patient.DateOfDeath)
            .ThenBy(patient => patient.LastName)
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
}
