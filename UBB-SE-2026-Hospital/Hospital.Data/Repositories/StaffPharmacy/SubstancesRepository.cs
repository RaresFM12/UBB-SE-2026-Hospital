using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class SubstancesRepository(HospitalDbContext context) : ISubstancesRepository
{
    public async Task<Substance?> GetByIdAsync(int substanceId)
        => await context.Substances.FindAsync(substanceId);

    public async Task<List<Substance>> GetAllAsync()
        => await context.Substances.ToListAsync();

    public async Task<Substance> CreateAsync(Substance substance)
    {
        context.Substances.Add(substance);
        await context.SaveChangesAsync();
        return substance;
    }

    public async Task<Substance> UpdateAsync(Substance substance)
    {
        context.Substances.Update(substance);
        await context.SaveChangesAsync();
        return substance;
    }

    public async Task DeleteAsync(int substanceId)
    {
        var substance = await context.Substances.FindAsync(substanceId);
        if (substance is not null)
        {
            context.Substances.Remove(substance);
            await context.SaveChangesAsync();
        }
    }
}
