using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class HangoutRepository(HospitalDbContext context) : IHangoutRepository
{
    public async Task<Hangout?> GetByIdAsync(int hangoutId)
        => await context.Hangouts
            .Include(h => h.HangoutParticipantEntries)
            .FirstOrDefaultAsync(h => h.HangoutID == hangoutId);

    public async Task<List<Hangout>> GetAllAsync()
        => await context.Hangouts.Include(h => h.HangoutParticipantEntries).ToListAsync();

    public async Task<List<Hangout>> GetByOrganizerIdAsync(int staffId)
        => await context.Hangouts
            .Where(h => h.OrganizerId == staffId)
            .ToListAsync();

    public async Task<Hangout> CreateAsync(Hangout hangout)
    {
        context.Hangouts.Add(hangout);
        await context.SaveChangesAsync();
        return hangout;
    }

    public async Task<Hangout> UpdateAsync(Hangout hangout)
    {
        context.Hangouts.Update(hangout);
        await context.SaveChangesAsync();
        return hangout;
    }

    public async Task DeleteAsync(int hangoutId)
    {
        var hangout = await context.Hangouts.FindAsync(hangoutId);
        if (hangout is not null)
        {
            context.Hangouts.Remove(hangout);
            await context.SaveChangesAsync();
        }
    }
}
