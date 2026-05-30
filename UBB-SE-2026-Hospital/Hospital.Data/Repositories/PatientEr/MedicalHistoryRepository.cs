using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class MedicalHistoryRepository(HospitalDbContext context) : IMedicalHistoryRepository
{
    public async Task<MedicalHistory?> GetByIdAsync(int medicalHistoryId)
        => await context.MedicalHistories
            .Include(m => m.PatientAllergies)
                .ThenInclude(pa => pa.Allergy)
            .Include(m => m.MedicalRecords)
            .FirstOrDefaultAsync(m => m.MedicalHistoryId == medicalHistoryId);

    public async Task<MedicalHistory?> GetByPatientIdAsync(int patientId)
        => await context.MedicalHistories
            .Include(m => m.PatientAllergies)
                .ThenInclude(pa => pa.Allergy)
            .Include(m => m.MedicalRecords)
            .FirstOrDefaultAsync(m => m.PatientId == patientId);

    public async Task<List<MedicalHistory>> GetAllAsync()
        => await context.MedicalHistories.ToListAsync();

    public async Task<MedicalHistory> CreateAsync(MedicalHistory medicalHistory)
    {
        context.MedicalHistories.Add(medicalHistory);
        await context.SaveChangesAsync();
        return medicalHistory;
    }

    public async Task<MedicalHistory> UpdateAsync(MedicalHistory medicalHistory)
    {
        context.MedicalHistories.Update(medicalHistory);
        await context.SaveChangesAsync();
        return medicalHistory;
    }

    public async Task DeleteAsync(int medicalHistoryId)
    {
        var history = await context.MedicalHistories.FindAsync(medicalHistoryId);
        if (history is not null)
        {
            context.MedicalHistories.Remove(history);
            await context.SaveChangesAsync();
        }
    }
}
