using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class HighRiskMedicineRepository(HospitalDbContext context) : IHighRiskMedicineRepository
{
    public async Task<HighRiskMedicine?> GetByIdAsync(int medicineId)
        => await context.HighRiskMedicines.FindAsync(medicineId);

    public async Task<List<HighRiskMedicine>> GetAllAsync()
        => await context.HighRiskMedicines.ToListAsync();

    public async Task<HighRiskMedicine> CreateAsync(HighRiskMedicine medicine)
    {
        context.HighRiskMedicines.Add(medicine);
        await context.SaveChangesAsync();
        return medicine;
    }

    public async Task<HighRiskMedicine> UpdateAsync(HighRiskMedicine medicine)
    {
        context.HighRiskMedicines.Update(medicine);
        await context.SaveChangesAsync();
        return medicine;
    }

    public async Task DeleteAsync(int medicineId)
    {
        var medicine = await context.HighRiskMedicines.FindAsync(medicineId);
        if (medicine is not null)
        {
            context.HighRiskMedicines.Remove(medicine);
            await context.SaveChangesAsync();
        }
    }
}
