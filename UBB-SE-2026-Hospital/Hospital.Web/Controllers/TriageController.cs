using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.Web.Models.Triage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize]
public class TriageController : Controller
{
    private const int DefaultNurseId = 2;

    private readonly ITriageService triageService;
    private readonly ITriageParametersService triageParametersService;
    private readonly ITriageDecisionService triageDecisionService;
    private readonly IERVisitService erVisitService;

    public TriageController(
        ITriageService triageService,
        ITriageParametersService triageParametersService,
        ITriageDecisionService triageDecisionService,
        IERVisitService erVisitService)
    {
        this.triageService = triageService;
        this.triageParametersService = triageParametersService;
        this.triageDecisionService = triageDecisionService;
        this.erVisitService = erVisitService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? selectedVisitId, CancellationToken cancellationToken)
    {
        try
        {
            TriageViewModel model = await BuildModelAsync(selectedVisitId, new TriageFormViewModel());
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
            TriageViewModel invalidModel = await BuildModelAsync(form.VisitId, form);
            return View("Index", invalidModel);
        }

        try
        {
            ERVisit visit = await erVisitService.GetByIdAsync(form.VisitId)
                ?? throw new KeyNotFoundException($"Visit {form.VisitId} was not found.");

            Triage? existingTriage = await triageService.GetByVisitIdAsync(form.VisitId);
            if (existingTriage is not null &&
                await triageParametersService.GetByTriageIdAsync(existingTriage.TriageId) is not null)
            {
                TempData["ErrorMessage"] = "Triage has already been performed for this visit.";
                return RedirectToAction(nameof(Index), new { selectedVisitId = form.VisitId });
            }

            if (!CanPerformTriage(visit.Status))
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

            Triage triage = existingTriage ?? new Triage { Visit = visit };
            triage.Visit = visit;
            triage.TriageLevel = triageDecisionService.CalculateTriageLevel(parameters);
            triage.Specialization = triageDecisionService.DetermineSpecialization(parameters);
            triage.NurseId = DefaultNurseId;
            triage.TriageTime = DateTime.Now;

            triage = existingTriage is null
                ? await triageService.CreateAsync(triage)
                : await triageService.UpdateAsync(triage);

            parameters.Triage = triage;
            await triageParametersService.CreateAsync(parameters);

            if (string.Equals(visit.Status, ERVisit.VisitStatus.REGISTERED, StringComparison.OrdinalIgnoreCase))
            {
                visit.Status = ERVisit.VisitStatus.TRIAGED;
                await erVisitService.UpdateAsync(visit);
            }

            TempData["SuccessMessage"] = $"Visit {form.VisitId} triaged as level {triage.TriageLevel} ({triage.Specialization}).";
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
            else if (await triageParametersService.GetByTriageIdAsync(triage.TriageId) is null)
            {
                TempData["ErrorMessage"] = "Triage parameters are missing. Re-run triage before moving the visit to the room queue.";
            }
            else
            {
                ERVisit? visit = await erVisitService.GetByIdAsync(visitId);
                if (visit is not null)
                {
                    visit.Status = ERVisit.VisitStatus.WAITING_FOR_ROOM;
                    await erVisitService.UpdateAsync(visit);
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
        TriageFormViewModel form)
    {
        List<ERVisit> visits = (await erVisitService.GetAllAsync())
            .Where(visit => CanPerformTriage(visit.Status))
            .OrderBy(visit => visit.ArrivalDateTime)
            .ToList();

        List<Triage> triages = await triageService.GetAllAsync();
        HashSet<int> triageIdsWithParameters = (await triageParametersService.GetAllAsync())
            .Where(parameters => parameters.Triage is not null)
            .Select(parameters => parameters.Triage.TriageId)
            .ToHashSet();

        form.VisitId = selectedVisitId ?? form.VisitId;
        Triage? selectedTriage = selectedVisitId.HasValue
            ? triages.FirstOrDefault(triage => triage.Visit is not null &&
                triage.Visit.VisitId == selectedVisitId.Value &&
                triageIdsWithParameters.Contains(triage.TriageId))
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
                Triage? triage = triages.FirstOrDefault(item => item.Visit is not null &&
                    item.Visit.VisitId == visit.VisitId &&
                    triageIdsWithParameters.Contains(item.TriageId));

                return new TriageVisitViewModel
                {
                    VisitId = visit.VisitId,
                    PatientId = visit.Patient.PatientId.ToString(),
                    ArrivalTime = visit.ArrivalDateTime,
                    ChiefComplaint = visit.ChiefComplaint,
                    Status = visit.Status,
                    TriageLevel = triage?.TriageLevel,
                    Specialization = triage?.Specialization
                };
            }).ToList()
        };
    }

    private static bool CanPerformTriage(string status) =>
        string.Equals(status, ERVisit.VisitStatus.REGISTERED, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, ERVisit.VisitStatus.TRIAGED, StringComparison.OrdinalIgnoreCase);

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before opening triage.";
        return RedirectToAction("Login", "Auth");
    }
}
