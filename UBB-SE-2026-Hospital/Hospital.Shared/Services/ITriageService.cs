using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;

namespace Hospital.Shared.Services;

public interface ITriageService
{
    Task<List<Triage>> GetAllAsync();
    Task<Triage?> GetByIdAsync(int id);
    Task<Triage> CreateAsync(Triage triage);
    Task<Triage> UpdateAsync(Triage triage);
    Task DeleteAsync(int id);

    // House-MD ER specific methods
    Task<Triage?> GetByVisitIdAsync(int visitId);
    Task<Triage> CreateTriageAsync(int visitId, PerformTriageDto parameters);
    Task MoveVisitToQueueAsync(int visitId);
    Task CloseVisitAsync(int visitId);
    Task<List<ERVisit>> GetVisitsForTriageAsync();
}
