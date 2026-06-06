using Hospital.Data.Models;
using Hospital.Data.Models;
using Hospital.Web.Models.Triage;
using Hospital.Shared.Services;
using Hospital.Web.Services;
using Hospital.Shared.Proxies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hospital.Services;
using Hospital.Web.Services;
using Hospital.Shared.Proxies;

namespace Hospital.Web.Controllers;

[Authorize]
public class TriageController : Controller
{
    private const int DefaultNurseId = 2;

    private readonly IErWorkflowApiClient erApiClient;

    public TriageController(IErWorkflowApiClient erApiClient)
    {
        this.erApiClient = erApiClient;
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
            var parameters = new TriageParameters
            {
                Consciousness = form.Consciousness,
                Breathing = form.Breathing,
                Bleeding = form.Bleeding,
                InjuryType = form.InjuryType,
                PainLevel = form.PainLevel
            };
            parameters.ValidateParameters();

            PerformTriageResponse result = await erApiClient.PerformTriageAsync(new PerformTriageRequest
            {
                VisitId = form.VisitId,
                NurseId = DefaultNurseId,
                TriageTime = DateTime.Now,
                Consciousness = parameters.Consciousness,
                Breathing = parameters.Breathing,
                Bleeding = parameters.Bleeding,
                InjuryType = parameters.InjuryType,
                PainLevel = parameters.PainLevel,
            }, cancellationToken);

            TempData["SuccessMessage"] = $"Visit {form.VisitId} triaged as level {result.Triage.TriageLevel} ({result.Triage.Specialization}).";
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
            await erApiClient.MoveVisitToQueueAsync(visitId, cancellationToken);
            TempData["SuccessMessage"] = $"Visit {visitId} is now waiting for a room.";
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
        form.VisitId = selectedVisitId ?? form.VisitId;
        ERVisit? selectedVisit = selectedVisitId.HasValue
            ? visits.FirstOrDefault(visit => visit.VisitId == selectedVisitId.Value)
            : null;
        Triage? selectedTriage = selectedVisit is not null
            && string.Equals(selectedVisit.Status, ERVisit.VisitStatus.TRIAGED, StringComparison.OrdinalIgnoreCase)
            ? triages.FirstOrDefault(triage => triage.Visit.VisitId == selectedVisit.VisitId)
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
                Triage? triage = string.Equals(
                    visit.Status,
                    ERVisit.VisitStatus.TRIAGED,
                    StringComparison.OrdinalIgnoreCase)
                    ? triages.FirstOrDefault(item => item.Visit.VisitId == visit.VisitId)
                    : null;
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

