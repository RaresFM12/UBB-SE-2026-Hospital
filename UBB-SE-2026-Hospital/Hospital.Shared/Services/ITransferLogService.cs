using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;

namespace Hospital.Shared.Services;

public interface ITransferLogService
{
    Task<List<TransferLog>> GetByVisitIdAsync(int visitId);
    Task<List<ERTransferEligibleVisit>> GetEligibleVisitsAsync();
    Task<TransferLog> CreateAsync(TransferLog log);
    Task UpdateAsync(TransferLog log);
}
