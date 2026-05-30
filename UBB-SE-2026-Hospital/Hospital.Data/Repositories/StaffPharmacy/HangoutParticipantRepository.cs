using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class HangoutParticipantRepository(HospitalDbContext context) : IHangoutParticipantRepository
{
    public async Task<HangoutParticipant?> GetByIdAsync(int hangoutId, int staffId)
        => await context.HangoutParticipants
            .FirstOrDefaultAsync(p => p.HangoutId == hangoutId && p.StaffId == staffId);

    public async Task<List<HangoutParticipant>> GetByHangoutIdAsync(int hangoutId)
        => await context.HangoutParticipants.Where(p => p.HangoutId == hangoutId).ToListAsync();

    public async Task<List<HangoutParticipant>> GetByStaffIdAsync(int staffId)
        => await context.HangoutParticipants.Where(p => p.StaffId == staffId).ToListAsync();

    public async Task<HangoutParticipant> CreateAsync(HangoutParticipant participant)
    {
        context.HangoutParticipants.Add(participant);
        await context.SaveChangesAsync();
        return participant;
    }

    public async Task DeleteAsync(int hangoutId, int staffId)
    {
        var participant = await GetByIdAsync(hangoutId, staffId);
        if (participant is not null)
        {
            context.HangoutParticipants.Remove(participant);
            await context.SaveChangesAsync();
        }
    }
}
