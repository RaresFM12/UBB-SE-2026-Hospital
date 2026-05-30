using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface ISubstancesRepository
{
    Task<Substance?> GetByIdAsync(int substanceId);
    Task<List<Substance>> GetAllAsync();
    Task<Substance> CreateAsync(Substance substance);
    Task<Substance> UpdateAsync(Substance substance);
    Task DeleteAsync(int substanceId);
}
