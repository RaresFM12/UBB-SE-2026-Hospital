using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface IStaffRepository
{
    Task<Staff?> GetByIdAsync(int staffId);
    Task<List<Staff>> GetAllAsync();
    Task<List<Doctor>> GetAllDoctorsAsync();
    Task<List<Pharmacyst>> GetAllPharmacistsAsync();
    Task<Staff> CreateAsync(Staff staff);
    Task<Staff> UpdateAsync(Staff staff);
    Task DeleteAsync(int staffId);
    Task<bool> ExistsAsync(int staffId);
}
