using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.Web.Models.Queue;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize]
public class QueueController : Controller
{
    private readonly IERVisitService erVisitService;
    private readonly ITriageService triageService;
    private readonly ITriageParametersService triageParametersService;

    public QueueController(
        IERVisitService erVisitService,
        ITriageService triageService,
        ITriageParametersService triageParametersService)
    {
        this.erVisitService = erVisitService;
        this.triageService = triageService;
        this.triageParametersService = triageParametersService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            List<ERVisit> waitingVisits = (await erVisitService.GetAllAsync())
                .Where(visit => string.Equals(visit.Status, ERVisit.VisitStatus.WAITING_FOR_ROOM, StringComparison.OrdinalIgnoreCase))
                .ToList();
            List<Triage> triages = await triageService.GetAllAsync();
            HashSet<int> triageIdsWithParameters = (await triageParametersService.GetAllAsync())
                .Where(parameters => parameters.Triage is not null)
                .Select(parameters => parameters.Triage.TriageId)
                .ToHashSet();

            return View(new QueueViewModel
            {
                ActiveVisits = waitingVisits
                    .Select(visit =>
                    {
                        Triage? triage = triages.FirstOrDefault(item => item.Visit is not null && item.Visit.VisitId == visit.VisitId);
                        bool hasTriageData = triage is not null && triageIdsWithParameters.Contains(triage.TriageId);

                        return new QueueItemViewModel
                        {
                            VisitId = visit.VisitId,
                            PatientId = visit.Patient.PatientId.ToString(),
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
            });
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
        return RedirectToAction("Login", "Auth");
    }
}
