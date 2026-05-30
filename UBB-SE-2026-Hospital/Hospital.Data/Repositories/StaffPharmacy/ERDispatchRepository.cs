using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class ERDispatchRepository(HospitalDbContext context) : IERDispatchRepository
{
    public async Task<ERRequest?> GetByIdAsync(int requestId)
        => await context.ERRequests.FindAsync(requestId);

    public async Task<List<ERRequest>> GetAllAsync()
        => await context.ERRequests.ToListAsync();

    public async Task<List<ERRequest>> GetPendingAsync()
        => await context.ERRequests.Where(r => r.Status == ERRequest.PendingStatus).ToListAsync();

    public async Task<List<ERRequest>> GetByDoctorIdAsync(int doctorId)
        => await context.ERRequests.Where(r => r.AssignedDoctorId == doctorId).ToListAsync();

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
