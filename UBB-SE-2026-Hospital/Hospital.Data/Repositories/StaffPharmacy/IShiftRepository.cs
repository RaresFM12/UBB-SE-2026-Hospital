using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface IShiftRepository
{
    Task<Shift?> GetByIdAsync(int shiftId);
    Task<List<Shift>> GetAllAsync();
    Task<List<Shift>> GetByStaffIdAsync(int staffId);
    Task<List<Shift>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<List<Shift>> GetCurrentShiftsAsync();
    Task<Shift> CreateAsync(Shift shift);
    Task<Shift> UpdateAsync(Shift shift);
    Task DeleteAsync(int shiftId);
}
