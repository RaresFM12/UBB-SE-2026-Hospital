using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services.PatientEr;

public class ERVisitService(
    IERVisitRepository erVisitRepository,
    IERRoomRepository erRoomRepository,
    ITriageRepository triageRepository,
    ITriageParametersRepository triageParametersRepository,
    ITransferLogRepository transferLogRepository,
    IPatientRepository patientRepository) : IERVisitService
{
    public Task<List<ERVisit>> GetAllAsync()
        => erVisitRepository.GetAllAsync();

    public Task<ERVisit?> GetByIdAsync(int id)
        => erVisitRepository.GetByIdAsync(id);

    public Task<List<ERVisit>> GetByPatientIdAsync(int patientId)
        => erVisitRepository.GetByPatientIdAsync(patientId);

    public Task<List<ERVisit>> GetActiveVisitsAsync()
        => erVisitRepository.GetActiveVisitsAsync();

    public Task<ERVisit> CreateAsync(ERVisit visit)
        => erVisitRepository.CreateAsync(visit);

    public async Task<ERVisit> UpdateAsync(ERVisit visit)
    {
        ERVisit current = await erVisitRepository.GetByIdAsync(visit.VisitId)
            ?? throw new ArgumentException($"ER visit {visit.VisitId} was not found.");

        if (visit.Patient is not null)
        {
            current.Patient = visit.Patient;
        }

        current.ArrivalDateTime = visit.ArrivalDateTime;
        current.ChiefComplaint = visit.ChiefComplaint;
        current.Status = visit.Status;

        return await erVisitRepository.UpdateAsync(current);
    }

    public Task DeleteAsync(int id)
        => erVisitRepository.DeleteAsync(id);

    public async Task<bool> AutoAssignHighestPriorityRoomAsync()
    {
        IReadOnlyList<(ERVisit Visit, Triage Triage)> waitingVisits = await GetWaitingVisitsWithTriageAsync();
        if (waitingVisits.Count == 0)
        {
            return false;
        }

        (ERVisit visit, Triage triage) = waitingVisits[0];
        TriageParameters? parameters = await triageParametersRepository.GetByTriageIdAsync(triage.TriageId);

        string requiredRoomType = DetermineRoomType(
            triage.Specialization,
            parameters?.Bleeding ?? 1,
            parameters?.InjuryType ?? 1,
            parameters?.Consciousness ?? 1,
            parameters?.Breathing ?? 1);

        ERRoom? room = (await erRoomRepository.GetAvailableRoomsAsync())
            .FirstOrDefault(item => item.RoomTypeName == requiredRoomType);

        if (room is null)
        {
            return false;
        }

        await AssignRoomAsync(visit.VisitId, room.RoomId);
        return true;
    }

    public async Task AssignRoomAsync(int visitId, int roomId)
    {
        ERRoom room = await erRoomRepository.GetByIdAsync(roomId)
            ?? throw new InvalidOperationException($"ER room {roomId} was not found.");

        if (!ERRoom.StatusEquals(room.AvailabilityStatus, ERRoom.RoomStatus.Available))
        {
            throw new InvalidOperationException($"ER room {roomId} is not available.");
        }

        ERVisit visit = await erVisitRepository.GetByIdAsync(visitId)
            ?? throw new InvalidOperationException($"ER visit {visitId} was not found.");

        EnsureVisitStatus(visit, ERVisit.VisitStatus.WAITING_FOR_ROOM);

        room.UpdateAvailabilityStatus(ERRoom.RoomStatus.Occupied);
        room.CurrentVisit = visit;
        visit.Status = ERVisit.VisitStatus.IN_ROOM;

        await erRoomRepository.UpdateAsync(room);
        await erVisitRepository.UpdateAsync(visit);
    }

    public async Task TransferVisitAsync(int visitId)
    {
        try
        {
            await LogTransferAsync(visitId, "SUCCESS");
            await UpdateVisitStatusAsync(visitId, ERVisit.VisitStatus.TRANSFERRED);
            await ReleaseRoomAsync(visitId);
            await MarkPatientAsTransferredAsync(visitId);
        }
        catch
        {
            await LogTransferAsync(visitId, "FAILED");
            throw;
        }
    }

    public async Task RetryTransferAsync(int visitId)
    {
        await LogTransferAsync(visitId, "RETRYING");
        await LogTransferAsync(visitId, "SUCCESS");
        await UpdateVisitStatusAsync(visitId, ERVisit.VisitStatus.TRANSFERRED);
        await ReleaseRoomAsync(visitId);
        await MarkPatientAsTransferredAsync(visitId);
    }

    public async Task CloseVisitAsync(int visitId)
    {
        await UpdateVisitStatusAsync(visitId, ERVisit.VisitStatus.CLOSED);
        await ReleaseRoomAsync(visitId);
    }

    private async Task<IReadOnlyList<(ERVisit Visit, Triage Triage)>> GetWaitingVisitsWithTriageAsync()
    {
        List<ERVisit> waitingVisits = (await erVisitRepository.GetAllAsync())
            .Where(visit => string.Equals(visit.Status, ERVisit.VisitStatus.WAITING_FOR_ROOM, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var results = new List<(ERVisit Visit, Triage Triage)>();

        foreach (ERVisit visit in waitingVisits)
        {
            Triage? triage = await triageRepository.GetByVisitIdAsync(visit.VisitId);
            if (triage is not null)
            {
                results.Add((visit, triage));
            }
        }

        return results
            .OrderBy(item => item.Triage.TriageLevel)
            .ThenBy(item => item.Visit.ArrivalDateTime)
            .ToList();
    }

    private static string DetermineRoomType(
        string? specialization,
        int bleeding,
        int injuryType,
        int consciousness,
        int breathing)
    {
        if (string.Equals(specialization, "General Surgery", StringComparison.OrdinalIgnoreCase)
            || bleeding == 3
            || injuryType == 3)
        {
            return ERRoom.RoomType.OperatingRoom;
        }

        if (consciousness == 3 || breathing == 3)
        {
            return ERRoom.RoomType.TraumaBay;
        }

        if (string.Equals(specialization, "Pulmonology", StringComparison.OrdinalIgnoreCase) || breathing == 2)
        {
            return ERRoom.RoomType.RespiratoryRoom;
        }

        if (string.Equals(specialization, "Neurology", StringComparison.OrdinalIgnoreCase) || consciousness == 2)
        {
            return ERRoom.RoomType.NeurologyRoom;
        }

        if (string.Equals(specialization, "Orthopedics", StringComparison.OrdinalIgnoreCase) || injuryType == 2)
        {
            return ERRoom.RoomType.OrthopedicRoom;
        }

        return ERRoom.RoomType.GeneralRoom;
    }

    private async Task UpdateVisitStatusAsync(int visitId, string nextStatus)
    {
        ERVisit visit = await erVisitRepository.GetByIdAsync(visitId)
            ?? throw new InvalidOperationException($"ER visit {visitId} was not found.");

        EnsureTransitionAllowed(visit.Status, nextStatus);
        visit.Status = nextStatus;
        await erVisitRepository.UpdateAsync(visit);
    }

    private static void EnsureVisitStatus(ERVisit visit, string expectedStatus)
    {
        if (!string.Equals(visit.Status, expectedStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"ER visit {visit.VisitId} is not in {expectedStatus} status.");
        }
    }

    private static void EnsureTransitionAllowed(string currentStatus, string nextStatus)
    {
        if (!ERVisit.ValidTransitions.TryGetValue(currentStatus, out List<string>? validNextStatuses)
            || !validNextStatuses.Contains(nextStatus))
        {
            throw new InvalidOperationException(
                $"Invalid ER visit status transition: '{currentStatus}' -> '{nextStatus}'.");
        }
    }

    private async Task LogTransferAsync(int visitId, string status)
    {
        ERVisit visit = await erVisitRepository.GetByIdAsync(visitId)
            ?? throw new InvalidOperationException($"ER visit {visitId} was not found.");

        await transferLogRepository.CreateAsync(new TransferLog
        {
            Visit = visit,
            TransferTime = DateTime.Now,
            TargetSystem = "Patient Management",
            Status = status,
        });
    }

    private async Task ReleaseRoomAsync(int visitId)
    {
        ERRoom? room = (await erRoomRepository.GetAllAsync())
            .FirstOrDefault(item => item.CurrentVisit?.VisitId == visitId);

        if (room is null)
        {
            return;
        }

        if (ERRoom.StatusEquals(room.AvailabilityStatus, ERRoom.RoomStatus.Occupied))
        {
            room.UpdateAvailabilityStatus(ERRoom.RoomStatus.Cleaning);
        }

        room.CurrentVisit = null;
        await erRoomRepository.UpdateAsync(room);
    }

    private async Task MarkPatientAsTransferredAsync(int visitId)
    {
        ERVisit visit = await erVisitRepository.GetByIdAsync(visitId)
            ?? throw new InvalidOperationException($"ER visit {visitId} was not found.");

        if (visit.Patient is null)
        {
            return;
        }

        visit.Patient.Transferred = true;
        await patientRepository.UpdateAsync(visit.Patient);
    }
}
