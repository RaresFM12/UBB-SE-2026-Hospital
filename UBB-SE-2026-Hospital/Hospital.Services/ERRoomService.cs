using Hospital.Data.Models;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services;

public class ERRoomService(
    IERRoomRepository erRoomRepository,
    IERVisitRepository erVisitRepository,
    ITriageRepository triageRepository) : IERRoomService
{
    public Task<List<ERRoom>> GetAllAsync()
        => erRoomRepository.GetAllAsync();

    public Task<ERRoom?> GetByIdAsync(int id)
        => erRoomRepository.GetByIdAsync(id);

    public Task<ERRoom> CreateAsync(ERRoom room)
        => erRoomRepository.CreateAsync(room);

    public async Task<ERRoom> UpdateAsync(ERRoom room)
    {
        ERRoom current = await erRoomRepository.GetByIdAsync(room.RoomId)
            ?? throw new ArgumentException($"ER room {room.RoomId} was not found.");

        current.RoomTypeName = room.RoomTypeName;
        current.AvailabilityStatus = room.AvailabilityStatus;
        current.CurrentVisit = room.CurrentVisit;

        return await erRoomRepository.UpdateAsync(current);
    }

    public Task DeleteAsync(int id)
        => erRoomRepository.DeleteAsync(id);

    public async Task<List<ERRoom>> GetByStatusAsync(string status)
    {
        List<ERRoom> rooms = await erRoomRepository.GetAllAsync();
        return rooms
            .Where(room => ERRoom.StatusEquals(room.AvailabilityStatus, status))
            .ToList();
    }

    public async Task<ERRoomVisitDetails?> GetVisitDetailsAsync(int roomId)
    {
        ERRoom room = await erRoomRepository.GetByIdAsync(roomId)
            ?? throw new ArgumentException($"ER room {roomId} was not found.");

        if (room.CurrentVisit is null)
        {
            return null;
        }

        ERVisit? visit = await erVisitRepository.GetByIdAsync(room.CurrentVisit.VisitId);
        if (visit is null)
        {
            return null;
        }

        Triage? triage = await triageRepository.GetByVisitIdAsync(visit.VisitId);

        return new ERRoomVisitDetails
        {
            Visit = visit,
            Patient = visit.Patient,
            Triage = triage,
        };
    }

    public async Task MarkRoomAsCleaningAsync(int roomId)
    {
        ERRoom room = await erRoomRepository.GetByIdAsync(roomId)
            ?? throw new ArgumentException($"ER room {roomId} was not found.");

        ERVisit? visit = room.CurrentVisit;

        room.UpdateAvailabilityStatus(ERRoom.RoomStatus.Cleaning);
        room.CurrentVisit = null;
        await erRoomRepository.UpdateAsync(room);

        if (visit is null)
        {
            return;
        }

        ERVisit? currentVisit = await erVisitRepository.GetByIdAsync(visit.VisitId);
        if (currentVisit is not null
            && (string.Equals(currentVisit.Status, ERVisit.VisitStatus.IN_ROOM, StringComparison.OrdinalIgnoreCase)
                || string.Equals(currentVisit.Status, ERVisit.VisitStatus.WAITING_FOR_DOCTOR, StringComparison.OrdinalIgnoreCase)))
        {
            currentVisit.Status = ERVisit.VisitStatus.WAITING_FOR_ROOM;
            await erVisitRepository.UpdateAsync(currentVisit);
        }
    }

    public async Task MarkRoomAsAvailableAsync(int roomId)
    {
        ERRoom room = await erRoomRepository.GetByIdAsync(roomId)
            ?? throw new ArgumentException($"ER room {roomId} was not found.");

        room.UpdateAvailabilityStatus(ERRoom.RoomStatus.Available);
        await erRoomRepository.UpdateAsync(room);
    }
}
