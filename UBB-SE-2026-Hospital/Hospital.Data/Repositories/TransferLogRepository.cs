using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class TransferLogRepository(HospitalDbContext context) : ITransferLogRepository
{
    public async Task<TransferLog?> GetByIdAsync(int transferLogId)
        => await context.TransferLogs
            .Include(transferLog => transferLog.Visit)
            .FirstOrDefaultAsync(transferLog => transferLog.TransferLogId == transferLogId);

    public async Task<List<TransferLog>> GetAllAsync()
        => await context.TransferLogs
            .Include(transferLog => transferLog.Visit)
            .ToListAsync();

    public async Task<List<TransferLog>> GetByVisitIdAsync(int visitId)
        => await context.TransferLogs
            .Include(transferLog => transferLog.Visit)
            .Where(transferLog => transferLog.Visit.VisitId == visitId)
            .OrderByDescending(transferLog => transferLog.TransferTime)
            .ToListAsync();

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
