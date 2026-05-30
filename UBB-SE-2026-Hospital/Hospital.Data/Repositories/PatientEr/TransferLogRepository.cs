using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class TransferLogRepository(HospitalDbContext context) : ITransferLogRepository
{
    public async Task<TransferLog?> GetByIdAsync(int transferLogId)
        => await context.TransferLogs.FindAsync(transferLogId);

    public async Task<List<TransferLog>> GetAllAsync()
        => await context.TransferLogs.ToListAsync();

    public async Task<List<TransferLog>> GetByVisitIdAsync(int visitId)
        => await context.TransferLogs.Where(t => t.Visit.VisitId == visitId).ToListAsync();

    public async Task<TransferLog> CreateAsync(TransferLog transferLog)
    {
        context.TransferLogs.Add(transferLog);
        await context.SaveChangesAsync();
        return transferLog;
    }

    public async Task<TransferLog> UpdateAsync(TransferLog transferLog)
    {
        context.TransferLogs.Update(transferLog);
        await context.SaveChangesAsync();
        return transferLog;
    }

    public async Task DeleteAsync(int transferLogId)
    {
        var log = await context.TransferLogs.FindAsync(transferLogId);
        if (log is not null)
        {
            context.TransferLogs.Remove(log);
            await context.SaveChangesAsync();
        }
    }
}
