using Hospital.Data.Models.DTOs;
using Hospital.Services.PatientEr;
using Hospital.Shared.Services;
using Hospital.Web.Models.BloodCompatibility;
using Hospital.Web.Models.Patients;
using Hospital.Web.Models.Transplant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DbPatient = Hospital.Data.Models.Patient;
using SharedPatient = Hospital.Shared.Models.PatientEr.Patient;

namespace Hospital.Web.Controllers;

[Authorize]
public class OrganDonorController : Controller
{
    private readonly IBloodCompatibilityService bloodCompatibilityService;
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
                Score = bloodCompatibilityService.CalculateScore(ToDbPatient(donor), ToDbPatient(recipientDetails))
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

    private static DbPatient ToDbPatient(SharedPatient patient)
    {
        return new DbPatient
        {
            PatientId = patient.PatientId,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Cnp = patient.Cnp,
            DateOfBirth = patient.DateOfBirth,
            DateOfDeath = patient.DateOfDeath,
            Sex = Enum.Parse<Hospital.Data.Models.Sex>(patient.Sex.ToString()),
            PhoneNumber = patient.PhoneNo,
            EmergencyContact = patient.EmergencyContact,
            IsArchived = patient.IsArchived,
            IsDonor = patient.IsDonor,
            Transferred = patient.Transferred,
            MedicalHistory = patient.MedicalHistory is null
                ? null
                : new Hospital.Data.Models.MedicalHistory
                {
                    BloodType = patient.MedicalHistory.BloodType,
                    Rh = patient.MedicalHistory.Rh
                }
        };
    }
}
