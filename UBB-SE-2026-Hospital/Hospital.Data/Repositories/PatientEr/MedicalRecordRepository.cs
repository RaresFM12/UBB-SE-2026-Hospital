using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class MedicalRecordRepository(HospitalDbContext context) : IMedicalRecordRepository
{
    public async Task<MedicalRecord?> GetByIdAsync(int recordId)
        => await context.MedicalRecords.FindAsync(recordId);

    public async Task<List<MedicalRecord>> GetByMedicalHistoryIdAsync(int medicalHistoryId)
        => await context.MedicalRecords
            .Where(r => r.MedicalHistoryId == medicalHistoryId)
            .ToListAsync();

    public async Task<List<MedicalRecord>> GetAllAsync()
        => await context.MedicalRecords.ToListAsync();

    public async Task<MedicalRecord> CreateAsync(MedicalRecord record)
    {
        context.MedicalRecords.Add(record);
        await context.SaveChangesAsync();
        return record;
    }

    public async Task<MedicalRecord> UpdateAsync(MedicalRecord record)
    {
        context.MedicalRecords.Update(record);
        await context.SaveChangesAsync();
        return record;
    }

    public async Task DeleteAsync(int recordId)
    {
        var record = await context.MedicalRecords.FindAsync(recordId);
        if (record is not null)
        {
            context.MedicalRecords.Remove(record);
            await context.SaveChangesAsync();
        }
    }
}
