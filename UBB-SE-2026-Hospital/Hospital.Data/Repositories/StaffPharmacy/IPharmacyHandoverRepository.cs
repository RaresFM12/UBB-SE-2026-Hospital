using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface IPharmacyHandoverRepository
{
    Task<PharmacyHandover?> GetByIdAsync(int handoverId);
    Task<List<PharmacyHandover>> GetAllAsync();
    Task<List<PharmacyHandover>> GetByPharmacistIdAsync(int pharmacistId);
    Task<PharmacyHandover> CreateAsync(PharmacyHandover handover);
    Task<PharmacyHandover> UpdateAsync(PharmacyHandover handover);
    Task DeleteAsync(int handoverId);
}
