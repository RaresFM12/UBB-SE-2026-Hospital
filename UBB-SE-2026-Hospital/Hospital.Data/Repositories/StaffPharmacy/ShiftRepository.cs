using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class ShiftRepository(HospitalDbContext context) : IShiftRepository
{
    public async Task<Shift?> GetByIdAsync(int shiftId)
        => await context.Shifts.Include(s => s.Staff).FirstOrDefaultAsync(s => s.Id == shiftId);

    public async Task<List<Shift>> GetAllAsync()
        => await context.Shifts.Include(s => s.Staff).ToListAsync();

    public async Task<List<Shift>> GetByStaffIdAsync(int staffId)
        => await context.Shifts.Where(s => s.StaffId == staffId).ToListAsync();

    public async Task<List<Shift>> GetByDateRangeAsync(DateTime start, DateTime end)
        => await context.Shifts
            .Where(s => s.StartTime >= start && s.EndTime <= end)
            .ToListAsync();

    public async Task<List<Shift>> GetCurrentShiftsAsync()
    {
        var now = DateTime.UtcNow;
        return await context.Shifts
            .Where(s => s.StartTime <= now && s.EndTime >= now && s.Status == ShiftStatus.ACTIVE)
            .ToListAsync();
    }

    public async Task<Shift> CreateAsync(Shift shift)
    {
        context.Shifts.Add(shift);
        await context.SaveChangesAsync();
        return shift;
    }

    public async Task<Shift> UpdateAsync(Shift shift)
    {
        context.Shifts.Update(shift);
        await context.SaveChangesAsync();
        return shift;
    }

    public async Task DeleteAsync(int shiftId)
    {
        var shift = await context.Shifts.FindAsync(shiftId);
        if (shift is not null)
        {
            context.Shifts.Remove(shift);
            await context.SaveChangesAsync();
        }
    }
}
