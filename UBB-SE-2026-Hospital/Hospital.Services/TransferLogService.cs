using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services;

public class TransferLogService(
    ITransferLogRepository transferLogRepository,
    IERVisitRepository erVisitRepository) : ITransferLogService
{
    public Task<List<TransferLog>> GetAllAsync()
        => transferLogRepository.GetAllAsync();

    public Task<TransferLog?> GetByIdAsync(int id)
        => transferLogRepository.GetByIdAsync(id);

    public Task<TransferLog> CreateAsync(TransferLog transferLog)
        => transferLogRepository.CreateAsync(transferLog);

    public async Task UpdateAsync(TransferLog transferLog)
        => await transferLogRepository.UpdateAsync(transferLog);

    public Task DeleteAsync(int id)
        => transferLogRepository.DeleteAsync(id);

    public Task<List<TransferLog>> GetByVisitIdAsync(int visitId)
        => transferLogRepository.GetByVisitIdAsync(visitId);

    public async Task<List<ERTransferEligibleVisit>> GetEligibleVisitsAsync()
    {
        List<ERVisit> visits = (await erVisitRepository.GetAllAsync())
            .Where(v =>
                string.Equals(v.Status, ERVisit.VisitStatus.IN_EXAMINATION, StringComparison.OrdinalIgnoreCase)
                || string.Equals(v.Status, ERVisit.VisitStatus.TRANSFERRED, StringComparison.OrdinalIgnoreCase))
            .OrderBy(v => string.Equals(
                v.Status,
                ERVisit.VisitStatus.TRANSFERRED,
                StringComparison.OrdinalIgnoreCase))
            .ThenBy(visit => visit.ArrivalDateTime)
            .ToList();

        return visits.Select(v => new ERTransferEligibleVisit
        {
            VisitId = v.VisitId,
            PatientName = v.Patient?.FullName ?? string.Empty,
            ChiefComplaint = v.ChiefComplaint,
            Status = v.Status,
            Transferred = v.Patient?.Transferred ?? false,
        }).ToList();
    }
}
