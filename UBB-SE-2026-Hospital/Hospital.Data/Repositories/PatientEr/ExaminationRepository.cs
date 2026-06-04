using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class ExaminationRepository(HospitalDbContext context) : IExaminationRepository
{
    public async Task<Examination?> GetByIdAsync(int examinationId)
        => await context.Examinations
            .Include(e => e.Visit)
            .ThenInclude(v => v.Patient)
            .Include(e => e.Doctor)
            .Include(e => e.Room)
            .FirstOrDefaultAsync(e => e.ExaminationId == examinationId);

    public async Task<List<Examination>> GetByVisitIdAsync(int visitId)
        => await context.Examinations
            .Include(e => e.Visit)
            .ThenInclude(v => v.Patient)
            .Include(e => e.Doctor)
            .Include(e => e.Room)
            .Where(e => e.Visit.VisitId == visitId)
            .ToListAsync();

    public async Task<List<Examination>> GetAllAsync()
        => await context.Examinations
            .Include(e => e.Visit)
            .ThenInclude(v => v.Patient)
            .Include(e => e.Doctor)
            .Include(e => e.Room)
            .ToListAsync();

    public async Task<Examination> CreateAsync(Examination examination)
    {
        AttachReferences(examination);
        context.Examinations.Add(examination);
        await context.SaveChangesAsync();
        return examination;
    }

    public async Task<Examination> UpdateAsync(Examination examination)
    {
        AttachReferences(examination);
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

    private void AttachReferences(Examination examination)
    {
        if (examination.Visit is not null)
        {
            context.Attach(examination.Visit);
        }

        if (examination.Doctor is not null)
        {
            context.Attach(examination.Doctor);
        }

        if (examination.Room is not null)
        {
            context.Attach(examination.Room);
        }
    }
}
