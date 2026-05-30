using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface IHighRiskMedicineRepository
{
    Task<HighRiskMedicine?> GetByIdAsync(int medicineId);
    Task<List<HighRiskMedicine>> GetAllAsync();
    Task<HighRiskMedicine> CreateAsync(HighRiskMedicine medicine);
    Task<HighRiskMedicine> UpdateAsync(HighRiskMedicine medicine);
    Task DeleteAsync(int medicineId);
}
