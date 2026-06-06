using Hospital.Data.Models;

namespace Hospital.Shared.Proxies;

public interface IERVisitApiClient
{
    Task<List<ERVisit>> GetAllAsync();
    Task<ERVisit?> GetByIdAsync(int id);
    Task<List<ERVisit>> GetByPatientIdAsync(int patientId);
    Task<List<ERVisit>> GetActiveVisitsAsync();
    Task<ERVisit> CreateAsync(ERVisit visit);
    Task<ERVisit> UpdateAsync(ERVisit visit);
    Task DeleteAsync(int id);
    Task<bool> AutoAssignHighestPriorityRoomAsync();
    Task AssignRoomAsync(int visitId, int roomId);
    Task TransferVisitAsync(int visitId);
    Task RetryTransferAsync(int visitId);
    Task CloseVisitAsync(int visitId);
    Task<List<ERVisit>> GetByStatusAsync(string status);
}
