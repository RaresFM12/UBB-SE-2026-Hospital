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
            .Include(participant => participant.Hangout)
            .Include(participant => participant.Staff)
            .FirstOrDefaultAsync(participant => participant.Hangout.HangoutID == hangoutId && participant.Staff.StaffId == staffId);

    public async Task<List<HangoutParticipant>> GetByHangoutIdAsync(int hangoutId)
        => await context.HangoutParticipants
            .Include(participant => participant.Hangout)
            .Include(participant => participant.Staff)
            .Where(participant => participant.Hangout.HangoutID == hangoutId)
            .ToListAsync();

    public async Task<List<HangoutParticipant>> GetByStaffIdAsync(int staffId)
        => await context.HangoutParticipants
            .Include(participant => participant.Hangout)
            .Include(participant => participant.Staff)
            .Where(participant => participant.Staff.StaffId == staffId)
            .ToListAsync();

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
