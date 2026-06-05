using Hospital.Data.Models.DTOs;
using Hospital.Data.Models;
using Hospital.Web.Models.Transfer;
using Hospital.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize]
public class TransferController : Controller
{
    private readonly IErWorkflowApiClient erApiClient;

    public TransferController(IErWorkflowApiClient erApiClient)
    {
        this.erApiClient = erApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? selectedVisitId, CancellationToken cancellationToken)
    {
        try
        {
            TransferViewModel model = await BuildModelAsync(selectedVisitId, cancellationToken);
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            return View(new TransferViewModel { ErrorMessage = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int visitId, CancellationToken cancellationToken)
    {
        try
        {
            await erApiClient.TransferVisitAsync(visitId, cancellationToken);
            TempData["SuccessMessage"] = $"Visit {visitId} was transferred to Patient Management.";
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Transfer failed: {ex.Message}";
            return RedirectToAction(nameof(Index), new { selectedVisitId = visitId });
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Retry(int visitId, CancellationToken cancellationToken)
    {
        try
        {
            await erApiClient.RetryTransferAsync(visitId, cancellationToken);
            TempData["SuccessMessage"] = $"Transfer retry for visit {visitId} succeeded.";
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Retry failed: {ex.Message}";
            return RedirectToAction(nameof(Index), new { selectedVisitId = visitId });
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int visitId, CancellationToken cancellationToken)
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
            TempData["ErrorMessage"] = $"Close failed: {ex.Message}";
            return RedirectToAction(nameof(Index), new { selectedVisitId = visitId });
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<TransferViewModel> BuildModelAsync(int? selectedVisitId, CancellationToken cancellationToken)
    {
        List<ERTransferEligibleVisit> eligibleVisits = await erApiClient.GetEligibleTransferVisitsAsync(cancellationToken);
        var model = new TransferViewModel
        {
            SelectedVisitId = selectedVisitId,
            EligibleVisits = eligibleVisits
                .Select(visit => new TransferVisitViewModel
                {
                    VisitId = visit.VisitId,
                    PatientName = visit.PatientName,
                    ChiefComplaint = visit.ChiefComplaint,
                    Status = visit.Status,
                    Transferred = visit.Transferred
                })
                .ToList()
        };

        if (!selectedVisitId.HasValue)
        {
            return model;
        }

        List<TransferLog> logs = await erApiClient.GetTransferLogsByVisitIdAsync(selectedVisitId.Value, cancellationToken);
        model.TransferLogs = logs
            .Select(log => new TransferLogItemViewModel
            {
                TransferId = log.TransferLogId,
                VisitId = log.Visit.VisitId,
                TransferTime = log.TransferTime,
                TargetSystem = log.TargetSystem,
                Status = log.Status
            })
            .ToList();
        model.CanRetry = model.TransferLogs.FirstOrDefault()?.Status == "FAILED";

        return model;
    }

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before opening transfers.";
        return RedirectToAction("AuthenticationView", "Authentication");
    }
}
