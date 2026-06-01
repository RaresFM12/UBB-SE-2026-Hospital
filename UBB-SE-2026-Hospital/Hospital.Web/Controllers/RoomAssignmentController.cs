using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.Web.Models.RoomAssignment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize]
public class RoomAssignmentController : Controller
{
    private readonly IERVisitService erVisitService;
    private readonly IERRoomService erRoomService;
    private readonly ITriageService triageService;
    private readonly ITriageParametersService triageParametersService;

    public RoomAssignmentController(
        IERVisitService erVisitService,
        IERRoomService erRoomService,
        ITriageService triageService,
        ITriageParametersService triageParametersService)
    {
        this.erVisitService = erVisitService;
        this.erRoomService = erRoomService;
        this.triageService = triageService;
        this.triageParametersService = triageParametersService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? selectedVisitId, CancellationToken cancellationToken)
    {
        try
        {
            RoomAssignmentViewModel model = await BuildModelAsync(selectedVisitId, null);
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            return View(new RoomAssignmentViewModel { ErrorMessage = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AutoAssign(CancellationToken cancellationToken)
    {
        try
        {
            bool assigned = await erVisitService.AutoAssignHighestPriorityRoomAsync();
            TempData[assigned ? "SuccessMessage" : "ErrorMessage"] = assigned
                ? "The highest-priority visit was assigned to a matching room."
                : "No suitable room is currently available for the highest-priority visit.";
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
    public async Task<IActionResult> Assign(int visitId, int roomId, CancellationToken cancellationToken)
    {
        if (visitId <= 0 || roomId <= 0)
        {
            TempData["ErrorMessage"] = "Select both a waiting visit and an available room.";
            return RedirectToAction(nameof(Index), new { selectedVisitId = visitId });
        }

        try
        {
            await erVisitService.AssignRoomAsync(visitId, roomId);
            TempData["SuccessMessage"] = $"Visit {visitId} was assigned to room {roomId}.";
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index), new { selectedVisitId = visitId });
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<RoomAssignmentViewModel> BuildModelAsync(
        int? selectedVisitId,
        int? selectedRoomId)
    {
        List<ERVisit> waitingVisits = (await erVisitService.GetAllAsync())
            .Where(visit => string.Equals(visit.Status, ERVisit.VisitStatus.WAITING_FOR_ROOM, StringComparison.OrdinalIgnoreCase))
            .ToList();
        List<Triage> triages = await triageService.GetAllAsync();
        HashSet<int> triageIdsWithParameters = (await triageParametersService.GetAllAsync())
            .Where(parameters => parameters.Triage is not null)
            .Select(parameters => parameters.Triage.TriageId)
            .ToHashSet();
        List<ERRoom> availableRooms = await erRoomService.GetByStatusAsync(ERRoom.RoomStatus.Available);

        var model = new RoomAssignmentViewModel
        {
            SelectedVisitId = selectedVisitId,
            SelectedRoomId = selectedRoomId,
            WaitingVisits = waitingVisits
                .Select(visit =>
                {
                    Triage? triage = triages.FirstOrDefault(item => item.Visit is not null && item.Visit.VisitId == visit.VisitId);
                    bool hasTriageData = triage is not null && triageIdsWithParameters.Contains(triage.TriageId);

                    return new RoomAssignmentVisitViewModel
                    {
                        VisitId = visit.VisitId,
                        PatientId = visit.Patient.PatientId.ToString(),
                        ArrivalTime = visit.ArrivalDateTime,
                        ChiefComplaint = visit.ChiefComplaint,
                        Status = visit.Status,
                        TriageLevel = triage?.TriageLevel,
                        Specialization = triage?.Specialization,
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
                .ToList(),
            AvailableRooms = availableRooms
                .OrderBy(room => room.RoomId)
                .Select(room => new RoomOptionViewModel
                {
                    RoomId = room.RoomId,
                    RoomType = room.RoomTypeName,
                    Status = room.AvailabilityStatus
                })
                .ToList()
        };

        if (!selectedVisitId.HasValue)
        {
            return model;
        }

        ERVisit? selectedVisit = waitingVisits.FirstOrDefault(visit => visit.VisitId == selectedVisitId.Value)
            ?? await erVisitService.GetByIdAsync(selectedVisitId.Value);
        if (selectedVisit is null)
        {
            return model;
        }

        model.SelectedPatient = new RoomAssignmentPatientViewModel
        {
            PatientId = selectedVisit.Patient.PatientId.ToString(),
            Name = selectedVisit.Patient.FullName,
            Phone = selectedVisit.Patient.PhoneNumber
        };

        Triage? selectedTriage = triages.FirstOrDefault(triage => triage.Visit is not null && triage.Visit.VisitId == selectedVisit.VisitId);
        bool selectedVisitHasParameters = selectedTriage is not null && triageIdsWithParameters.Contains(selectedTriage.TriageId);
        if (selectedTriage is not null)
        {
            model.SelectedTriage = new RoomAssignmentTriageViewModel
            {
                TriageLevel = selectedTriage.TriageLevel,
                Specialization = selectedTriage.Specialization,
                NurseId = selectedTriage.NurseId
            };
        }

        if (!selectedVisitHasParameters)
        {
            model.ErrorMessage = selectedTriage is null
                ? "The selected visit cannot be assigned to a room because its triage record is missing."
                : "The selected visit cannot be assigned to a room because its triage parameters are missing.";
        }

        return model;
    }

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before opening room assignment.";
        return RedirectToAction("Login", "Auth");
    }
}
