using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class PrescriptionRepository(HospitalDbContext context) : IPrescriptionRepository
{
    public async Task<Prescription?> GetByIdAsync(int prescriptionId)
        => await context.Prescriptions
            .Include(p => p.MedicationList)
            .FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId);

    public async Task<List<Prescription>> GetAllAsync()
        => await context.Prescriptions
            .Include(p => p.MedicationList)
            .ToListAsync();

    public async Task<List<Prescription>> GetFilteredAsync(PrescriptionFilter filter)
    {
        var query = context.Prescriptions
            .Include(p => p.MedicationList)
            .AsQueryable();

        if (filter.PrescriptionId.HasValue)
            query = query.Where(p => p.PrescriptionId == filter.PrescriptionId.Value);

        if (filter.DateFrom.HasValue)
            query = query.Where(p => p.Date >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(p => p.Date <= filter.DateTo.Value);

        if (!string.IsNullOrWhiteSpace(filter.DoctorName))
            query = query.Where(p => p.DoctorName.Contains(filter.DoctorName));

        return await query.ToListAsync();
    }

    public async Task<List<Prescription>> GetByRecordIdAsync(int recordId)
        => await context.Prescriptions
            .Include(p => p.MedicationList)
            .Where(p => p.RecordId == recordId)
            .ToListAsync();

    public async Task<List<Prescription>> GetPotentialDrugAddictsAsync()
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);
        return await context.Prescriptions
            .Include(p => p.MedicationList)
            .Where(p => p.Date >= cutoff)
            .GroupBy(p => p.RecordId)
            .Where(g => g.Count() >= 5)
            .SelectMany(g => g)
            .ToListAsync();
    }

    public async Task<Prescription> CreateAsync(Prescription prescription)
    {
        context.Prescriptions.Add(prescription);
        await context.SaveChangesAsync();
        return prescription;
    }

    public async Task<Prescription> UpdateAsync(Prescription prescription)
    {
        context.Prescriptions.Update(prescription);
        await context.SaveChangesAsync();
        return prescription;
    }

    public async Task DeleteAsync(int prescriptionId)
    {
        var prescription = await context.Prescriptions.FindAsync(prescriptionId);
        if (prescription is not null)
        {
            context.Prescriptions.Remove(prescription);
            await context.SaveChangesAsync();
        }
    }
}
