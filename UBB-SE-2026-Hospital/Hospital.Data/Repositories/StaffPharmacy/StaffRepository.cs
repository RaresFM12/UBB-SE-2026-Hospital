using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class StaffRepository(HospitalDbContext context) : IStaffRepository
{
    public async Task<Staff?> GetByIdAsync(int staffId)
        => await context.Staff.FindAsync(staffId);

    public async Task<List<Staff>> GetAllAsync()
        => await context.Staff.ToListAsync();

    public async Task<List<Doctor>> GetAllDoctorsAsync()
        => await context.Staff.OfType<Doctor>().ToListAsync();

    public async Task<List<Pharmacyst>> GetAllPharmacistsAsync()
        => await context.Staff.OfType<Pharmacyst>().ToListAsync();

    public async Task<Staff> CreateAsync(Staff staff)
    {
        context.Staff.Add(staff);
        await context.SaveChangesAsync();
        return staff;
    }

    public async Task<Staff> UpdateAsync(Staff staff)
    {
        context.Staff.Update(staff);
        await context.SaveChangesAsync();
        return staff;
    }

    public async Task DeleteAsync(int staffId)
    {
        var staff = await context.Staff.FindAsync(staffId);
        if (staff is not null)
        {
            context.Staff.Remove(staff);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int staffId)
        => await context.Staff.AnyAsync(s => s.StaffID == staffId);
}
