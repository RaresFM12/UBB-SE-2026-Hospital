using Hospital.Data.Models;
using Hospital.Data.Models;
using Hospital.Web.Models.Triage;
using Hospital.Shared.Services;
using Hospital.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hospital.Services;
using Hospital.Web.Services;

namespace Hospital.Web.Controllers;

[Authorize]
public class TriageController : Controller
{
    private readonly IErWorkflowApiClient erApiClient;
    private readonly IErStaffService erStaffService;
    private readonly ITriageDecisionService triageDecisionService;

    public TriageController(
        IErWorkflowApiClient erApiClient,
        IErStaffService erStaffService,
        ITriageDecisionService triageDecisionService)
    {
        this.erApiClient = erApiClient;
        this.erStaffService = erStaffService;
        this.triageDecisionService = triageDecisionService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? selectedVisitId, CancellationToken cancellationToken)
    {
        try
        {
            TriageViewModel model = await BuildModelAsync(selectedVisitId, new TriageFormViewModel(), cancellationToken);
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            return View(new TriageViewModel { ErrorMessage = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Perform(
        [Bind(Prefix = "Form")] TriageFormViewModel form,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TriageViewModel invalidModel = await BuildModelAsync(form.VisitId, form, cancellationToken);
            return View("Index", invalidModel);
        }

        try
        {
            ERVisit visit = await erApiClient.GetVisitAsync(form.VisitId, cancellationToken)
                ?? throw new KeyNotFoundException($"Visit {form.VisitId} was not found.");

            Triage? existingTriage = await erApiClient.GetTriageByVisitIdAsync(form.VisitId, cancellationToken);
            if (existingTriage is not null &&
                await erApiClient.GetTriageParametersByTriageIdAsync(existingTriage.TriageId, cancellationToken) is not null)
            {
                TempData["ErrorMessage"] = "Triage has already been performed for this visit.";
                return RedirectToAction(nameof(Index), new { selectedVisitId = form.VisitId });
            }

            if (!string.Equals(visit.Status, ERVisit.VisitStatus.REGISTERED, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(visit.Status, ERVisit.VisitStatus.TRIAGED, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = $"Visit {form.VisitId} cannot be triaged while it is in status {visit.Status}.";
                return RedirectToAction(nameof(Index), new { selectedVisitId = form.VisitId });
            }

            var parameters = new TriageParameters
            {
                Consciousness = form.Consciousness,
                Breathing = form.Breathing,
                Bleeding = form.Bleeding,
                InjuryType = form.InjuryType,
                PainLevel = form.PainLevel
            };
            parameters.ValidateParameters();

            int nurseId = erStaffService.RequestAvailableNurse()
                ?? throw new InvalidOperationException("No available nurse.");

            var triage = new Triage
            {
                Visit = visit,
                TriageLevel = triageDecisionService.CalculateTriageLevel(parameters),
                Specialization = triageDecisionService.DetermineSpecialization(parameters),
                NurseId = nurseId,
                TriageTime = DateTime.Now
            };

            Triage createdTriage = await erApiClient.CreateTriageAsync(triage, cancellationToken);
            parameters.Triage = createdTriage;
            await erApiClient.CreateTriageParametersAsync(parameters, cancellationToken);
            await erApiClient.UpdateVisitStatusAsync(form.VisitId, ERVisit.VisitStatus.TRIAGED, cancellationToken);

            TempData["SuccessMessage"] = $"Visit {form.VisitId} triaged as level {createdTriage.TriageLevel} ({createdTriage.Specialization}).";
            return RedirectToAction(nameof(Index), new { selectedVisitId = form.VisitId });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index), new { selectedVisitId = form.VisitId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveToQueue(int visitId, CancellationToken cancellationToken)
    {
        try
        {
            Triage? triage = await erApiClient.GetTriageByVisitIdAsync(visitId, cancellationToken);
            if (triage is null)
            {
                TempData["ErrorMessage"] = "Perform triage before moving the visit to the room queue.";
            }
            else if (await erApiClient.GetTriageParametersByTriageIdAsync(triage.TriageId, cancellationToken) is null)
            {
                TempData["ErrorMessage"] = "Triage parameters are missing. Re-run triage before moving the visit to the room queue.";
            }
            else
            {
                ERVisit? visit = await erApiClient.GetVisitAsync(visitId, cancellationToken);
                if (visit != null)
                {
                    visit.Status = ERVisit.VisitStatus.WAITING_FOR_ROOM;
                    await erApiClient.UpdateVisitAsync(visitId, visit, cancellationToken);
                    TempData["SuccessMessage"] = $"Visit {visitId} is now waiting for a room.";
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseVisit(int visitId, CancellationToken cancellationToken)
    {
        try
        {
            await erApiClient.CloseVisitAsync(visitId, cancellationToken);
            TempData["SuccessMessage"] = $"Visit {visitId} was closed.";
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<TriageViewModel> BuildModelAsync(
        int? selectedVisitId,
        TriageFormViewModel form,
        CancellationToken cancellationToken)
    {
        List<ERVisit> visits = (await erApiClient.GetVisitsAsync(cancellationToken))
            .Where(visit =>
                string.Equals(visit.Status, ERVisit.VisitStatus.REGISTERED, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(visit.Status, ERVisit.VisitStatus.TRIAGED, StringComparison.OrdinalIgnoreCase))
            .OrderBy(visit => visit.ArrivalDateTime)
            .ToList();

        List<Triage> triages = await erApiClient.GetTriagesAsync(cancellationToken);
        List<TriageParameters> triageParameters = await erApiClient.GetTriageParametersAsync(cancellationToken);

        form.VisitId = selectedVisitId ?? form.VisitId;
        Triage? selectedTriage = selectedVisitId.HasValue
            ? triages.FirstOrDefault(triage => triage.Visit.VisitId == selectedVisitId.Value &&
                triageParameters.Any(parameters => parameters.Triage.TriageId == triage.TriageId))
            : null;

        return new TriageViewModel
        {
            SelectedVisitId = selectedVisitId,
            Form = form,
            SelectedTriage = selectedTriage is null
                ? null
                : new TriageResultViewModel
                {
                    TriageId = selectedTriage.TriageId,
                    TriageLevel = selectedTriage.TriageLevel,
                    Specialization = selectedTriage.Specialization,
                    NurseId = selectedTriage.NurseId,
                    TriageTime = selectedTriage.TriageTime
                },
            Visits = visits.Select(visit =>
            {
                Triage? triage = triages.FirstOrDefault(item => item.Visit.VisitId == visit.VisitId &&
                    triageParameters.Any(parameters => parameters.Triage.TriageId == item.TriageId));
                return new TriageVisitViewModel
                {
                    VisitId = visit.VisitId,
                    PatientId = visit.Patient.Cnp,
                    ArrivalTime = visit.ArrivalDateTime,
                    ChiefComplaint = visit.ChiefComplaint,
                    Status = visit.Status,
                    TriageLevel = triage?.TriageLevel,
                    Specialization = triage?.Specialization
                };
            }).ToList()
        };
    }

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before opening triage.";
        return RedirectToAction("AuthenticationView", "Authentication");
    }
}
