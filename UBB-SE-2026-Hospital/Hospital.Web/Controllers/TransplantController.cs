using Hospital.Data.Models;
using Hospital.Shared.Proxies;
using Hospital.Web.Models.Transplant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize(Roles = "Admin,Doctor,Nurse")]
public class TransplantController : Controller
{
    private readonly ITransplantApiClient transplantService;
    private readonly IPatientApiClient patientService;

    public TransplantController(ITransplantApiClient transplantService, IPatientApiClient patientService)
    {
        this.transplantService = transplantService;
        this.patientService = patientService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new TransplantLookupViewModel
        {
            SuccessMessage = this.TempData["SuccessMessage"]?.ToString(),
        });
    }

    [HttpGet]
    public async Task<IActionResult> Request(string? patientId, CancellationToken cancellationToken)
    {
        if (!int.TryParse(patientId, out int parsedPatientId) || parsedPatientId <= 0)
        {
            return this.LookupError(patientId, "Enter a valid positive patient ID.");
        }

        Patient? patient;
        try
        {
            patient = await this.patientService.GetByIdAsync(parsedPatientId, cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            return this.LookupError(patientId, "You do not have permission to access this patient.");
        }
        catch (InvalidOperationException)
        {
            return this.LookupError(
                patientId,
                "Patient information is temporarily unavailable. Please try again.");
        }

        if (patient is null)
        {
            return this.LookupError(patientId, $"No patient was found with ID {parsedPatientId}.");
        }

        var model = new TransplantRequestViewModel
        {
            PatientId = parsedPatientId,
            PatientName = patient.FullName,
        };

        await this.PopulatePatientIndicatorsAsync(model, cancellationToken);
        return this.View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Request(
        TransplantRequestViewModel model,
        CancellationToken cancellationToken)
    {
        async Task<IActionResult> ReturnWithErrorsAsync()
        {
            Patient? patient;
            try
            {
                patient = await this.patientService.GetByIdAsync(model.PatientId, cancellationToken);
            }
            catch (UnauthorizedAccessException)
            {
                return this.LookupError(
                    model.PatientId.ToString(),
                    "You do not have permission to access this patient.");
            }
            catch (InvalidOperationException)
            {
                return this.LookupError(
                    model.PatientId.ToString(),
                    "Patient information is temporarily unavailable. Please try again.");
            }

            if (patient is null)
            {
                return this.LookupError(
                    model.PatientId.ToString(),
                    $"No patient was found with ID {model.PatientId}.");
            }

            model.PatientName = patient.FullName;
            await this.PopulatePatientIndicatorsAsync(model, cancellationToken);
            return this.View(model);
        }

        if (!this.ModelState.IsValid)
        {
            return await ReturnWithErrorsAsync();
        }

        Patient? patient;
        try
        {
            patient = await this.patientService.GetByIdAsync(model.PatientId, cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            return this.LookupError(
                model.PatientId.ToString(),
                "You do not have permission to access this patient.");
        }
        catch (InvalidOperationException)
        {
            return this.LookupError(
                model.PatientId.ToString(),
                "Patient information is temporarily unavailable. Please try again.");
        }

        if (patient is null)
        {
            return this.LookupError(
                model.PatientId.ToString(),
                $"No patient was found with ID {model.PatientId}.");
        }

        try
        {
            await this.transplantService.CreateWaitlistRequestAsync(
                model.PatientId,
                model.SelectedOrgan!,
                cancellationToken);

            this.TempData["SuccessMessage"] =
                $"{patient.FullName} was added to the organ transplant waitlist.";

            return this.RedirectToAction(nameof(this.Index));
        }
        catch (ArgumentException exception)
        {
            this.ModelState.AddModelError(string.Empty, exception.Message);
            return await ReturnWithErrorsAsync();
        }
        catch (InvalidOperationException exception)
        {
            this.ModelState.AddModelError(string.Empty, exception.Message);
            return await ReturnWithErrorsAsync();
        }
    }

    private ViewResult LookupError(string? patientId, string message)
    {
        return this.View("Index", new TransplantLookupViewModel
        {
            PatientId = patientId ?? string.Empty,
            ErrorMessage = message,
        });
    }

    private async Task PopulatePatientIndicatorsAsync(
        TransplantRequestViewModel model,
        CancellationToken cancellationToken)
    {
        List<string> unavailableIndicators = new();

        try
        {
            model.IsUrgent = await this.transplantService.IsUrgentAsync(
                model.PatientId,
                cancellationToken);
        }
        catch (InvalidOperationException)
        {
            unavailableIndicators.Add("urgency");
        }

        try
        {
            model.WarningMessage = await this.transplantService.GetChronicWarningAsync(
                model.PatientId,
                cancellationToken);
        }
        catch (InvalidOperationException)
        {
            unavailableIndicators.Add("chronic-condition");
        }

        if (unavailableIndicators.Count > 0)
        {
            model.StatusMessage =
                $"The patient was loaded, but {string.Join(" and ", unavailableIndicators)} information is temporarily unavailable.";
        }
    }
}
