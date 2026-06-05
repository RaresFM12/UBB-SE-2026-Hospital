using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class ShiftSwapRepository(HospitalDbContext context) : IShiftSwapRepository
{
    public async Task<ShiftSwapRequest?> GetByIdAsync(int requestId)
        => await context.ShiftSwapRequests
            .Include(r => r.Shift)
                .ThenInclude(s => s!.Staff)
            .Include(r => r.Requester)
            .Include(r => r.Colleague)
            .FirstOrDefaultAsync(r => r.SwapId == requestId);

    public async Task<List<ShiftSwapRequest>> GetAllAsync()
        => await context.ShiftSwapRequests
            .Include(r => r.Shift)
                .ThenInclude(s => s!.Staff)
            .Include(r => r.Requester)
            .Include(r => r.Colleague)
            .ToListAsync();

    public async Task<List<ShiftSwapRequest>> GetByStaffIdAsync(int staffId)
        => await context.ShiftSwapRequests
            .Include(r => r.Shift)
                .ThenInclude(s => s!.Staff)
            .Include(r => r.Requester)
            .Include(r => r.Colleague)
            .Where(r => r.Requester!.StaffId == staffId || r.Colleague!.StaffId == staffId)
            .ToListAsync();

    public async Task<List<ShiftSwapRequest>> GetPendingAsync()
        => await context.ShiftSwapRequests
            .Include(r => r.Shift)
                .ThenInclude(s => s!.Staff)
            .Include(r => r.Requester)
            .Include(r => r.Colleague)
            .Where(r => r.Status == ShiftSwapRequestStatus.PENDING)
            .ToListAsync();

    public async Task<ShiftSwapRequest> CreateAsync(ShiftSwapRequest request)
    {
        context.ShiftSwapRequests.Add(request);
        await context.SaveChangesAsync();
        return request;
    }

    public async Task<ShiftSwapRequest> UpdateAsync(ShiftSwapRequest request)
    {
        context.ShiftSwapRequests.Update(request);
        await context.SaveChangesAsync();
        return request;
    }

    public async Task DeleteAsync(int requestId)
    {
        var request = await context.ShiftSwapRequests.FindAsync(requestId);
        if (request is not null)
        {
            context.ShiftSwapRequests.Remove(request);
            await context.SaveChangesAsync();
        }
    }
}
