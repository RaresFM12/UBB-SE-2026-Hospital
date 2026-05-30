using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface ITransferLogRepository
{
    Task<TransferLog?> GetByIdAsync(int transferLogId);
    Task<List<TransferLog>> GetAllAsync();
    Task<List<TransferLog>> GetByVisitIdAsync(int visitId);
    Task<TransferLog> CreateAsync(TransferLog transferLog);
    Task<TransferLog> UpdateAsync(TransferLog transferLog);
    Task DeleteAsync(int transferLogId);
}
