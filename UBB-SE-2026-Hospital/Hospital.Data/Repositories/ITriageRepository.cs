using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface ITriageRepository
{
    Task<Triage?> GetByIdAsync(int triageId);
    Task<Triage?> GetByVisitIdAsync(int visitId);
    Task<List<Triage>> GetAllAsync();
    Task<Triage> CreateAsync(Triage triage);
    Task<Triage> UpdateAsync(Triage triage);
    Task DeleteAsync(int triageId);
}
