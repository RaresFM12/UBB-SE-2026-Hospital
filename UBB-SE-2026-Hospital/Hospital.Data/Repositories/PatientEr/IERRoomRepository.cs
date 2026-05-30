using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface IERRoomRepository
{
    Task<ERRoom?> GetByIdAsync(int roomId);
    Task<List<ERRoom>> GetAllAsync();
    Task<List<ERRoom>> GetAvailableRoomsAsync();
    Task<ERRoom> CreateAsync(ERRoom room);
    Task<ERRoom> UpdateAsync(ERRoom room);
    Task DeleteAsync(int roomId);
}
