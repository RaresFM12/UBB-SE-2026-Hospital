using Hospital.Data.Models;
using Hospital.Data.Models;
using Hospital.Web.Models.RoomManagement;
using Hospital.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize]
public class RoomManagementController : Controller
{
    private readonly IErWorkflowApiClient erApiClient;

    public RoomManagementController(IErWorkflowApiClient erApiClient)
    {
        this.erApiClient = erApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? selectedRoomId, CancellationToken cancellationToken)
    {
        try
        {
            RoomManagementViewModel model = await BuildModelAsync(selectedRoomId, cancellationToken);
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
            await erApiClient.MarkRoomAsCleaningAsync(roomId, cancellationToken);
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
            await erApiClient.MarkRoomAsAvailableAsync(roomId, cancellationToken);
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

    private async Task<RoomManagementViewModel> BuildModelAsync(int? selectedRoomId, CancellationToken cancellationToken)
    {
        List<ERRoom> availableRooms = await erApiClient.GetRoomsByStatusAsync(ERRoom.RoomStatus.Available, cancellationToken);
        List<ERRoom> occupiedRooms = await erApiClient.GetRoomsByStatusAsync(ERRoom.RoomStatus.Occupied, cancellationToken);
        List<ERRoom> cleaningRooms = await erApiClient.GetRoomsByStatusAsync(ERRoom.RoomStatus.Cleaning, cancellationToken);

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

        ERRoomVisitDetails? visitDetails = await erApiClient.GetRoomVisitDetailsAsync(selectedRoomId.Value, cancellationToken);
        if (visitDetails?.Visit is null)
        {
            return model;
        }

        model.SelectedRoomVisit = new RoomVisitDetailsViewModel
        {
            VisitId = visitDetails.Visit.VisitId,
            PatientId = visitDetails.Visit.Patient.Cnp,
            PatientName = visitDetails.Patient?.FullName ?? visitDetails.Visit.Patient.FullName,
            ChiefComplaint = visitDetails.Visit.ChiefComplaint,
            VisitStatus = visitDetails.Visit.Status,
            TriageLevel = visitDetails.Triage?.TriageLevel,
            Specialization = visitDetails.Triage?.Specialization
        };

        return model;
    }

    private static RoomStatusItemViewModel MapRoom(ERRoom room) =>
        new ()
        {
            RoomId = room.RoomId,
            RoomType = room.RoomTypeName,
            Status = room.AvailabilityStatus,
            CurrentVisitId = room.CurrentVisit?.VisitId
        };

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before opening room management.";
        return RedirectToAction("AuthenticationView", "Authentication");
    }
}
