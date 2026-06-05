using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class ERDispatchRepository(HospitalDbContext context) : IERDispatchRepository
{
    public async Task<ERRequest?> GetByIdAsync(int requestId)
        => await context.ERRequests
            .Include(request => request.AssignedDoctor)
            .FirstOrDefaultAsync(request => request.Id == requestId);

    public async Task<List<ERRequest>> GetAllAsync()
        => await context.ERRequests
            .Include(request => request.AssignedDoctor)
            .ToListAsync();

    public async Task<List<ERRequest>> GetPendingAsync()
        => await context.ERRequests
            .Include(request => request.AssignedDoctor)
            .Where(request => request.Status == ERRequest.PendingStatus)
            .ToListAsync();

    public async Task<List<ERRequest>> GetByDoctorIdAsync(int doctorId)
        => await context.ERRequests
            .Include(request => request.AssignedDoctor)
            .Where(request => request.AssignedDoctor!.StaffId == doctorId)
            .ToListAsync();

    public async Task<ERRequest> CreateAsync(ERRequest request)
    {
        context.ERRequests.Add(request);
        await context.SaveChangesAsync();
        return request;
    }

    public async Task<ERRequest> UpdateAsync(ERRequest request)
    {
        context.ERRequests.Update(request);
        await context.SaveChangesAsync();
        return request;
    }

    public async Task DeleteAsync(int requestId)
    {
        var request = await context.ERRequests.FindAsync(requestId);
        if (request is not null)
        {
            context.ERRequests.Remove(request);
            await context.SaveChangesAsync();
        }
    }
}
