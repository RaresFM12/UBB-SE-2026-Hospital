using Hospital.Data.Models;
using Hospital.Web.Models.Queue;
using Hospital.Web.Services;
using Hospital.Shared.Proxies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize]
public class QueueController : Controller
{
    private readonly IErWorkflowApiClient erApiClient;

    public QueueController(IErWorkflowApiClient erApiClient)
    {
        this.erApiClient = erApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            List<ERVisit> waitingVisits = await erApiClient.GetVisitsByStatusAsync(
                ERVisit.VisitStatus.WAITING_FOR_ROOM,
                cancellationToken);
            List<Triage> triages = await erApiClient.GetTriagesAsync(cancellationToken);
            List<TriageParameters> triageParameters = await erApiClient.GetTriageParametersAsync(cancellationToken);
            HashSet<int> triageIdsWithParameters = triageParameters
                .Select(parameters => parameters.Triage.TriageId)
                .ToHashSet();

            var model = new QueueViewModel
            {
                ActiveVisits = waitingVisits
                    .Select(visit =>
                    {
                        Triage? triage = triages.FirstOrDefault(item => item.Visit.VisitId == visit.VisitId);
                        bool hasTriageData = triage is not null && triageIdsWithParameters.Contains(triage.TriageId);

                        return new QueueItemViewModel
                        {
                            VisitId = visit.VisitId,
                            PatientId = visit.Patient.Cnp,
                            TriageLevel = triage?.TriageLevel,
                            Specialization = triage?.Specialization,
                            ArrivalTime = visit.ArrivalDateTime,
                            Status = visit.Status,
                            HasTriageData = hasTriageData,
                            WarningMessage = hasTriageData
                                ? null
                                : triage is null
                                    ? "Triage record is missing."
                                    : "Triage parameters are missing."
                        };
                    })
                    .OrderBy(item => item.TriageLevel ?? int.MaxValue)
                    .ThenBy(item => item.ArrivalTime)
                    .ToList()
            };

            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            return View(new QueueViewModel { ErrorMessage = ex.Message });
        }
    }

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before opening the ER queue.";
        return RedirectToAction("AuthenticationView", "Authentication");
    }
}

