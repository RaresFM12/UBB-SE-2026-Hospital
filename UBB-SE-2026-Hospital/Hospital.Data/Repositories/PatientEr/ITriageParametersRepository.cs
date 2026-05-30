using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface ITriageParametersRepository
{
    Task<TriageParameters?> GetByIdAsync(int triageParametersId);
    Task<TriageParameters?> GetByTriageIdAsync(int triageId);
    Task<List<TriageParameters>> GetAllAsync();
    Task<TriageParameters> CreateAsync(TriageParameters triageParameters);
    Task<TriageParameters> UpdateAsync(TriageParameters triageParameters);
    Task DeleteAsync(int triageParametersId);
}
