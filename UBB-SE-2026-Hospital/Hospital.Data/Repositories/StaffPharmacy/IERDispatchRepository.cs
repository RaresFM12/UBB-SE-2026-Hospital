using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface IERDispatchRepository
{
    Task<ERRequest?> GetByIdAsync(int requestId);
    Task<List<ERRequest>> GetAllAsync();
    Task<List<ERRequest>> GetPendingAsync();
    Task<List<ERRequest>> GetByDoctorIdAsync(int doctorId);
    Task<ERRequest> CreateAsync(ERRequest request);
    Task<ERRequest> UpdateAsync(ERRequest request);
    Task DeleteAsync(int requestId);
}
