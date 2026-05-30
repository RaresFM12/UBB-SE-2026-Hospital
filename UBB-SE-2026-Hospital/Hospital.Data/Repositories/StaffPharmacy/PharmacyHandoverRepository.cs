using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class PharmacyHandoverRepository(HospitalDbContext context) : IPharmacyHandoverRepository
{
    public async Task<PharmacyHandover?> GetByIdAsync(int handoverId)
        => await context.PharmacyHandovers.FindAsync(handoverId);

    public async Task<List<PharmacyHandover>> GetAllAsync()
        => await context.PharmacyHandovers.ToListAsync();

    public async Task<List<PharmacyHandover>> GetByPharmacistIdAsync(int pharmacistId)
        => await context.PharmacyHandovers
            .Where(h => h.PharmacistId == pharmacistId)
            .ToListAsync();

    public async Task<PharmacyHandover> CreateAsync(PharmacyHandover handover)
    {
        context.PharmacyHandovers.Add(handover);
        await context.SaveChangesAsync();
        return handover;
    }

    public async Task<PharmacyHandover> UpdateAsync(PharmacyHandover handover)
    {
        context.PharmacyHandovers.Update(handover);
        await context.SaveChangesAsync();
        return handover;
    }

    public async Task DeleteAsync(int handoverId)
    {
        var handover = await context.PharmacyHandovers.FindAsync(handoverId);
        if (handover is not null)
        {
            context.PharmacyHandovers.Remove(handover);
            await context.SaveChangesAsync();
        }
    }
}
