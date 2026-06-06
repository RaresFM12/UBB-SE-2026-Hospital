using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class ERVisitRepository(HospitalDbContext context) : IERVisitRepository
{
    public async Task<ERVisit?> GetByIdAsync(int visitId)
        => await context.ERVisits
            .Include(visit => visit.Patient)
            .FirstOrDefaultAsync(visit => visit.VisitId == visitId);

    public async Task<List<ERVisit>> GetAllAsync()
        => await context.ERVisits.Include(visit => visit.Patient).ToListAsync();

    public async Task<List<ERVisit>> GetByPatientIdAsync(int patientId)
        => await context.ERVisits.Where(visit => visit.Patient.PatientId == patientId).ToListAsync();

    public async Task<List<ERVisit>> GetActiveVisitsAsync()
        => await context.ERVisits
            .Include(visit => visit.Patient)
            .Where(visit => visit.Status != ERVisit.VisitStatus.CLOSED && visit.Status != ERVisit.VisitStatus.TRANSFERRED)
            .ToListAsync();

    public async Task<ERVisit> CreateAsync(ERVisit visit)
    {
        context.ERVisits.Add(visit);
        await context.SaveChangesAsync();
        return visit;
    }

    public async Task<ERVisit> UpdateAsync(ERVisit visit)
    {
        context.ERVisits.Update(visit);
        await context.SaveChangesAsync();
        return visit;
    }

    public async Task DeleteAsync(int visitId)
    {
        var visit = await context.ERVisits.FindAsync(visitId);
        if (visit is not null)
        {
            context.ERVisits.Remove(visit);
            await context.SaveChangesAsync();
        }
    }
}
