using Common.Data.Models;
using Common.Data.Entity.DTOs;
using Hospital.Web.Models.Triage;
using Hospital.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize]
public class TriageController : Controller
{
    private readonly ITriageService triageService;
    private readonly IERVisitService erVisitService;
    private readonly IErStaffService erStaffService;

    public TriageController(ITriageService triageService, IERVisitService erVisitService, IErStaffService erStaffService)
    {
        this.triageService = triageService;
        this.erVisitService = erVisitService;
        this.erStaffService = erStaffService;
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
            ER_Visit visit = await erVisitService.GetByIdAsync(form.VisitId)
                ?? throw new KeyNotFoundException($"Visit {form.VisitId} was not found.");

            Triage? existingTriage = await triageService.GetByVisitIdAsync(form.VisitId);
            if (existingTriage is not null &&
                await triageService.GetParametersByTriageIdAsync(existingTriage.Triage_ID) is not null)
            {
                TempData["ErrorMessage"] = "Triage has already been performed for this visit.";
                return RedirectToAction(nameof(Index), new { selectedVisitId = form.VisitId });
            }

            if (!string.Equals(visit.Status, ER_Visit.VisitStatus.REGISTERED, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(visit.Status, ER_Visit.VisitStatus.TRIAGED, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = $"Visit {form.VisitId} cannot be triaged while it is in status {visit.Status}.";
                return RedirectToAction(nameof(Index), new { selectedVisitId = form.VisitId });
            }

            var parameters = new Triage_Parameters
            {
                Consciousness = form.Consciousness,
                Breathing = form.Breathing,
                Bleeding = form.Bleeding,
                Injury_Type = form.InjuryType,
                Pain_Level = form.PainLevel
            };
            parameters.ValidateParameters();

            int nurseId = erStaffService.RequestAvailableNurse()
                ?? throw new InvalidOperationException("No available nurse.");

            var request = new PerformTriageRequestDto
            {
                VisitId = form.VisitId,
                NurseId = nurseId,
                TriageTime = DateTime.Now,
                Consciousness = parameters.Consciousness,
                Breathing = parameters.Breathing,
                Bleeding = parameters.Bleeding,
                InjuryType = parameters.Injury_Type,
                PainLevel = parameters.Pain_Level
            };

            PerformTriageResponseDto result = await triageService.PerformTriageAsync(request);

            TempData["SuccessMessage"] = $"Visit {form.VisitId} triaged as level {result.Triage.Triage_Level} ({result.Triage.Specialization}).";
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
            Triage? triage = await triageService.GetByVisitIdAsync(visitId);
            if (triage is null)
            {
                TempData["ErrorMessage"] = "Perform triage before moving the visit to the room queue.";
            }
            else if (await triageService.GetParametersByTriageIdAsync(triage.Triage_ID) is null)
            {
                TempData["ErrorMessage"] = "Triage parameters are missing. Re-run triage before moving the visit to the room queue.";
            }
            else
            {
                var visit = await erVisitService.GetByIdAsync(visitId);
                if (visit != null)
                {
                    visit.Status = ER_Visit.VisitStatus.WAITING_FOR_ROOM;
                    await erVisitService.UpdateAsync(visitId, visit);
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
            await erVisitService.CloseVisitAsync(visitId);
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
        List<ER_Visit> visits = (await erVisitService.GetAllAsync())
            .Where(visit =>
                string.Equals(visit.Status, ER_Visit.VisitStatus.REGISTERED, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(visit.Status, ER_Visit.VisitStatus.TRIAGED, StringComparison.OrdinalIgnoreCase))
            .OrderBy(visit => visit.Arrival_date_time)
            .ToList();

        List<Triage> triages = await triageService.GetAllAsync();
        List<Triage_Parameters> triageParameters = await triageService.GetAllParametersAsync();

        form.VisitId = selectedVisitId ?? form.VisitId;
        Triage? selectedTriage = selectedVisitId.HasValue
            ? triages.FirstOrDefault(triage => triage.Visit_ID == selectedVisitId.Value &&
                triageParameters.Any(parameters => parameters.Triage_ID == triage.Triage_ID))
            : null;

        return new TriageViewModel
        {
            SelectedVisitId = selectedVisitId,
            Form = form,
            SelectedTriage = selectedTriage is null
                ? null
                : new TriageResultViewModel
                {
                    TriageId = selectedTriage.Triage_ID,
                    TriageLevel = selectedTriage.Triage_Level,
                    Specialization = selectedTriage.Specialization,
                    NurseId = selectedTriage.Nurse_ID,
                    TriageTime = selectedTriage.Triage_Time
                },
            Visits = visits.Select(visit =>
            {
                Triage? triage = triages.FirstOrDefault(item => item.Visit_ID == visit.Visit_ID &&
                    triageParameters.Any(parameters => parameters.Triage_ID == item.Triage_ID));
                return new TriageVisitViewModel
                {
                    VisitId = visit.Visit_ID,
                    PatientId = visit.Patient_ID,
                    ArrivalTime = visit.Arrival_date_time,
                    ChiefComplaint = visit.Chief_Complaint,
                    Status = visit.Status,
                    TriageLevel = triage?.Triage_Level,
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