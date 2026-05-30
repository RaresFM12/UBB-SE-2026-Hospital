using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface IAllergyRepository
{
    Task<Allergy?> GetByIdAsync(int allergyId);
    Task<List<Allergy>> GetAllAsync();
    Task<Allergy> CreateAsync(Allergy allergy);
    Task DeleteAsync(int allergyId);
}
