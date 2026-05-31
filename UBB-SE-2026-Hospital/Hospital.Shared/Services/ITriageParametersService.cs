using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface ITriageParametersService
{
    Task<List<TriageParameters>> GetAllAsync();
    Task<TriageParameters?> GetByIdAsync(int id);
    Task<TriageParameters?> GetByTriageIdAsync(int triageId);
    Task<TriageParameters> CreateAsync(TriageParameters parameters);
    Task<TriageParameters> UpdateAsync(TriageParameters parameters);
    Task DeleteAsync(int id);
}
