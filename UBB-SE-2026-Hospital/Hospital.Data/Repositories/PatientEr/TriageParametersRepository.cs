using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class TriageParametersRepository(HospitalDbContext context) : ITriageParametersRepository
{
    public async Task<TriageParameters?> GetByIdAsync(int triageParametersId)
        => await context.TriageParameters.FindAsync(triageParametersId);

    public async Task<TriageParameters?> GetByTriageIdAsync(int triageId)
        => await context.TriageParameters.FirstOrDefaultAsync(tp => tp.TriageId == triageId);

    public async Task<List<TriageParameters>> GetAllAsync()
        => await context.TriageParameters.ToListAsync();

    public async Task<TriageParameters> CreateAsync(TriageParameters triageParameters)
    {
        context.TriageParameters.Add(triageParameters);
        await context.SaveChangesAsync();
        return triageParameters;
    }

    public async Task<TriageParameters> UpdateAsync(TriageParameters triageParameters)
    {
        context.TriageParameters.Update(triageParameters);
        await context.SaveChangesAsync();
        return triageParameters;
    }

    public async Task DeleteAsync(int triageParametersId)
    {
        var tp = await context.TriageParameters.FindAsync(triageParametersId);
        if (tp is not null)
        {
            context.TriageParameters.Remove(tp);
            await context.SaveChangesAsync();
        }
    }
}
