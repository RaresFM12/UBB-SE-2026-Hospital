using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class EvaluationsRepository(HospitalDbContext context) : IEvaluationsRepository
{
    public async Task<MedicalEvaluation?> GetByIdAsync(int evaluationId)
        => await context.MedicalEvaluations.FindAsync(evaluationId);

    public async Task<List<MedicalEvaluation>> GetAllAsync()
        => await context.MedicalEvaluations.ToListAsync();

    public async Task<List<MedicalEvaluation>> GetByDoctorIdAsync(int doctorId)
        => await context.MedicalEvaluations.Where(e => e.EvaluatorId == doctorId).ToListAsync();

    public async Task<MedicalEvaluation> CreateAsync(MedicalEvaluation evaluation)
    {
        context.MedicalEvaluations.Add(evaluation);
        await context.SaveChangesAsync();
        return evaluation;
    }

    public async Task<MedicalEvaluation> UpdateAsync(MedicalEvaluation evaluation)
    {
        context.MedicalEvaluations.Update(evaluation);
        await context.SaveChangesAsync();
        return evaluation;
    }

    public async Task DeleteAsync(int evaluationId)
    {
        var evaluation = await context.MedicalEvaluations.FindAsync(evaluationId);
        if (evaluation is not null)
        {
            context.MedicalEvaluations.Remove(evaluation);
            await context.SaveChangesAsync();
        }
    }
}
