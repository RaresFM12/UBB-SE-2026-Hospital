using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface ITriageService
{
    Task<List<Triage>> GetAllAsync();
    Task<Triage?> GetByIdAsync(int id);
    Task<Triage> CreateAsync(Triage triage);
    Task<Triage> UpdateAsync(Triage triage);
    Task DeleteAsync(int id);
}
