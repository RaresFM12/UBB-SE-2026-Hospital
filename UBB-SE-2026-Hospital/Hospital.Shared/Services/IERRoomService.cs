using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;

namespace Hospital.Shared.Services;

public interface IERRoomService
{
    Task<List<ERRoom>> GetAllAsync();
    Task<ERRoom?> GetByIdAsync(int id);
    Task<ERRoom> CreateAsync(ERRoom room);
    Task<ERRoom> UpdateAsync(ERRoom room);
    Task DeleteAsync(int id);
    Task<List<ERRoom>> GetByStatusAsync(string status);
    Task<ERRoomVisitDetails?> GetVisitDetailsAsync(int roomId);
    Task MarkRoomAsCleaningAsync(int roomId);
    Task MarkRoomAsAvailableAsync(int roomId);
}
