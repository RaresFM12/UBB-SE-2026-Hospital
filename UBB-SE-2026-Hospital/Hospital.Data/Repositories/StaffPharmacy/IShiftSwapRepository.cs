using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface IShiftSwapRepository
{
    Task<ShiftSwapRequest?> GetByIdAsync(int requestId);
    Task<List<ShiftSwapRequest>> GetAllAsync();
    Task<List<ShiftSwapRequest>> GetByStaffIdAsync(int staffId);
    Task<List<ShiftSwapRequest>> GetPendingAsync();
    Task<ShiftSwapRequest> CreateAsync(ShiftSwapRequest request);
    Task<ShiftSwapRequest> UpdateAsync(ShiftSwapRequest request);
    Task DeleteAsync(int requestId);
}
