using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;

namespace Hospital.Services.PatientEr;

public interface ITransferLogService
{
    Task<List<TransferLog>> GetAllAsync();
    Task<TransferLog?> GetByIdAsync(int id);
    Task<TransferLog> CreateAsync(TransferLog transferLog);
    Task<TransferLog> UpdateAsync(TransferLog transferLog);
    Task DeleteAsync(int id);
    Task<List<TransferLog>> GetByVisitIdAsync(int visitId);
    Task<List<ERTransferEligibleVisit>> GetEligibleVisitsAsync();
}
