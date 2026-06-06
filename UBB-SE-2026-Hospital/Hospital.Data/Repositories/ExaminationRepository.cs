using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class ExaminationRepository(HospitalDbContext context) : IExaminationRepository
{
    public async Task<Examination?> GetByIdAsync(int examinationId)
        => await WithDetails()
            .FirstOrDefaultAsync(examination => examination.ExaminationId == examinationId);

    public async Task<List<Examination>> GetByVisitIdAsync(int visitId)
        => await WithDetails()
            .Where(examination => examination.Visit.VisitId == visitId)
            .ToListAsync();

    public async Task<List<Examination>> GetAllAsync()
        => await WithDetails().ToListAsync();

    public async Task<Examination> CreateAsync(Examination examination)
    {
        context.Examinations.Add(examination);
        await context.SaveChangesAsync();
        return examination;
    }

    public async Task<Examination> UpdateAsync(Examination examination)
    {
        context.Examinations.Update(examination);
        await context.SaveChangesAsync();
        return examination;
    }

    public async Task DeleteAsync(int examinationId)
    {
        var examination = await context.Examinations.FindAsync(examinationId);
        if (examination is not null)
        {
            context.Examinations.Remove(examination);
            await context.SaveChangesAsync();
        }
    }

    private IQueryable<Examination> WithDetails() =>
        context.Examinations
            .Include(examination => examination.Visit)
            .ThenInclude(visit => visit.Patient)
            .Include(examination => examination.Doctor)
            .Include(examination => examination.Room);
}
