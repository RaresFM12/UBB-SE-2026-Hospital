using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class TransplantRepository(HospitalDbContext context) : ITransplantRepository
{
    public async Task<Transplant?> GetByIdAsync(int transplantId)
        => await context.Transplants.FindAsync(transplantId);

    public async Task<List<Transplant>> GetAllAsync()
        => await context.Transplants.ToListAsync();

    public async Task<List<Transplant>> GetByPatientIdAsync(int patientId)
        => await context.Transplants
            .Where(t => t.Receiver.PatientId == patientId || t.Donor!.PatientId == patientId)
            .ToListAsync();

    public async Task<List<TransplantMatch>> GetMatchesForTransplantAsync(int transplantId)
        => await context.TransplantMatches
            .Where(m => m.Transplant.TransplantId == transplantId)
            .ToListAsync();

    public async Task<Transplant> CreateAsync(Transplant transplant)
    {
        context.Transplants.Add(transplant);
        await context.SaveChangesAsync();
        return transplant;
    }

    public async Task<Transplant> UpdateAsync(Transplant transplant)
    {
        context.Transplants.Update(transplant);
        await context.SaveChangesAsync();
        return transplant;
    }

    public async Task DeleteAsync(int transplantId)
    {
        var transplant = await context.Transplants.FindAsync(transplantId);
        if (transplant is not null)
        {
            context.Transplants.Remove(transplant);
            await context.SaveChangesAsync();
        }
    }

    public async Task<TransplantMatch> CreateMatchAsync(TransplantMatch match)
    {
        context.TransplantMatches.Add(match);
        await context.SaveChangesAsync();
        return match;
    }

    public async Task<TransplantMatch> UpdateMatchAsync(TransplantMatch match)
    {
        context.TransplantMatches.Update(match);
        await context.SaveChangesAsync();
        return match;
    }
}
