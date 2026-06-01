using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Shared.Services;
using Hospital.Web.Models.RoomManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize]
public class RoomManagementController : Controller
{
    private readonly IERRoomService erRoomService;

    public RoomManagementController(IERRoomService erRoomService)
    {
        this.erRoomService = erRoomService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? selectedRoomId, CancellationToken cancellationToken)
    {
        try
        {
            RoomManagementViewModel model = await BuildModelAsync(selectedRoomId);
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            return View(new RoomManagementViewModel { ErrorMessage = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkCleaning(int roomId, CancellationToken cancellationToken)
    {
        try
        {
            await erRoomService.MarkRoomAsCleaningAsync(roomId);
            TempData["SuccessMessage"] = $"Room {roomId} is now marked for cleaning.";
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
    public async Task<IActionResult> MarkAvailable(int roomId, CancellationToken cancellationToken)
    {
        try
        {
            await erRoomService.MarkRoomAsAvailableAsync(roomId);
            TempData["SuccessMessage"] = $"Room {roomId} is available again.";
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

    private async Task<RoomManagementViewModel> BuildModelAsync(int? selectedRoomId)
    {
        List<ERRoom> availableRooms = await erRoomService.GetByStatusAsync(ERRoom.RoomStatus.Available);
        List<ERRoom> occupiedRooms = await erRoomService.GetByStatusAsync(ERRoom.RoomStatus.Occupied);
        List<ERRoom> cleaningRooms = await erRoomService.GetByStatusAsync(ERRoom.RoomStatus.Cleaning);

        var model = new RoomManagementViewModel
        {
            SelectedRoomId = selectedRoomId,
            AvailableRooms = availableRooms.Select(MapRoom).ToList(),
            OccupiedRooms = occupiedRooms.Select(MapRoom).ToList(),
            CleaningRooms = cleaningRooms.Select(MapRoom).ToList()
        };

        if (!selectedRoomId.HasValue)
        {
            return model;
        }

        ERRoomVisitDetails? visitDetails = await erRoomService.GetVisitDetailsAsync(selectedRoomId.Value);
        if (visitDetails?.Visit is null)
        {
            return model;
        }

        model.SelectedRoomVisit = new RoomVisitDetailsViewModel
        {
            VisitId = visitDetails.Visit.VisitId,
            PatientId = visitDetails.Visit.Patient.PatientId.ToString(),
            PatientName = visitDetails.Patient?.FullName ?? visitDetails.Visit.Patient.PatientId.ToString(),
            ChiefComplaint = visitDetails.Visit.ChiefComplaint,
            VisitStatus = visitDetails.Visit.Status,
            TriageLevel = visitDetails.Triage?.TriageLevel,
            Specialization = visitDetails.Triage?.Specialization
        };

        return model;
    }

    private static RoomStatusItemViewModel MapRoom(ERRoom room) =>
        new()
        {
            RoomId = room.RoomId,
            RoomType = room.RoomTypeName,
            Status = room.AvailabilityStatus,
            CurrentVisitId = room.CurrentVisit?.VisitId
        };

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before opening room management.";
        return RedirectToAction("Login", "Auth");
    }
}
