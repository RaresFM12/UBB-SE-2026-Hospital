using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class TriageRepository(HospitalDbContext context) : ITriageRepository
{
    public async Task<Triage?> GetByIdAsync(int triageId)
        => await context.Triages.FindAsync(triageId);

    public async Task<Triage?> GetByVisitIdAsync(int visitId)
        => await context.Triages.FirstOrDefaultAsync(t => t.Visit.VisitId == visitId);

    public async Task<List<Triage>> GetAllAsync()
        => await context.Triages.ToListAsync();

    public async Task<Triage> CreateAsync(Triage triage)
    {
        context.Triages.Add(triage);
        await context.SaveChangesAsync();
        return triage;
    }

    public async Task<Triage> UpdateAsync(Triage triage)
    {
        context.Triages.Update(triage);
        await context.SaveChangesAsync();
        return triage;
    }

    public async Task DeleteAsync(int triageId)
    {
        var triage = await context.Triages.FindAsync(triageId);
        if (triage is not null)
        {
            context.Triages.Remove(triage);
            await context.SaveChangesAsync();
        }
    }
}
