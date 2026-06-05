using Hospital.Data.Models;
using Hospital.Web.Models.RoomAssignment;
using Hospital.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize]
public class RoomAssignmentController : Controller
{
    private const int InvalidEntityId = 0;

    private readonly IErWorkflowApiClient erApiClient;
    private readonly IPatientApiClient patientApiClient;

    public RoomAssignmentController(IErWorkflowApiClient erApiClient, IPatientApiClient patientApiClient)
    {
        this.erApiClient = erApiClient;
        this.patientApiClient = patientApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? selectedVisitId, CancellationToken cancellationToken)
    {
        try
        {
            RoomAssignmentViewModel model = await BuildModelAsync(selectedVisitId, null, cancellationToken);
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
            bool assigned = await erApiClient.AutoAssignHighestPriorityRoomAsync(cancellationToken);
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
        if (visitId <= InvalidEntityId || roomId <= InvalidEntityId)
        {
            TempData["ErrorMessage"] = "Select both a waiting visit and an available room.";
            return RedirectToAction(nameof(Index), new { selectedVisitId = visitId });
        }

        try
        {
            await erApiClient.AssignRoomAsync(visitId, roomId, cancellationToken);
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
        int? selectedRoomId,
        CancellationToken cancellationToken)
    {
        List<ERVisit> waitingVisits = await erApiClient.GetVisitsByStatusAsync(
            ERVisit.VisitStatus.WAITING_FOR_ROOM,
            cancellationToken);
        List<Triage> triages = await erApiClient.GetTriagesAsync(cancellationToken);
        List<TriageParameters> triageParameters = await erApiClient.GetTriageParametersAsync(cancellationToken);
        HashSet<int> triageIdsWithParameters = triageParameters
            .Select(parameters => parameters.Triage.TriageId)
            .ToHashSet();
        List<ERRoom> availableRooms = await erApiClient.GetRoomsByStatusAsync(
            ERRoom.RoomStatus.Available,
            cancellationToken);

        var model = new RoomAssignmentViewModel
        {
            SelectedVisitId = selectedVisitId,
            SelectedRoomId = selectedRoomId,
            WaitingVisits = waitingVisits
                .Select(visit =>
                {
                    Triage? triage = triages.FirstOrDefault(item => item.Visit.VisitId == visit.VisitId);
                    bool hasTriageData = triage is not null && triageIdsWithParameters.Contains(triage.TriageId);

                    return new RoomAssignmentVisitViewModel
                    {
                        VisitId = visit.VisitId,
                        PatientId = visit.Patient.Cnp,
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
            ?? await erApiClient.GetVisitAsync(selectedVisitId.Value, cancellationToken);
        if (selectedVisit is null)
        {
            return model;
        }

        Patient? patient = selectedVisit.Patient;

        model.SelectedPatient = new RoomAssignmentPatientViewModel
        {
            PatientId = patient.Cnp,
            Name = patient.FullName,
            Phone = patient.PhoneNumber
        };

        Triage? selectedTriage = triages.FirstOrDefault(triage => triage.Visit.VisitId == selectedVisit.VisitId);
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
        return RedirectToAction("AuthenticationView", "Authentication");
    }
}
