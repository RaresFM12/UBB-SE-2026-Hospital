using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface ITriageService
{
    Task<List<Triage>> GetAllAsync();
    Task<Triage?> GetByIdAsync(int id);
    Task<Triage> CreateAsync(Triage triage);
    Task<Triage> UpdateAsync(Triage triage);
    Task DeleteAsync(int id);
    Task<Triage?> GetByVisitIdAsync(int visitId);
    Task<Triage> CreateTriageAsync(int visitId, Hospital.Data.Models.PerformTriageRequest parameters);
    Task MoveVisitToQueueAsync(int visitId);
    Task CloseVisitAsync(int visitId);
    Task<List<Hospital.Data.Models.ERVisit>> GetVisitsForTriageAsync();
}
