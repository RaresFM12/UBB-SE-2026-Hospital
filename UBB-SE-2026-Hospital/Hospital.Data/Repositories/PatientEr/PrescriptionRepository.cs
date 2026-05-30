using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
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
            .Where(p => p.MedicalRecord.RecordId == recordId)
            .ToListAsync();

    public async Task<List<Prescription>> GetPotentialDrugAddictsAsync()
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);
        return await context.Prescriptions
            .Include(p => p.MedicationList)
            .Where(p => p.Date >= cutoff)
            .GroupBy(p => p.MedicalRecord.RecordId)
            .Where(g => g.Count() >= 5)
            .SelectMany(g => g)
            .ToListAsync();
    }

    public async Task<List<Prescription>> GetTopNAsync(int n, int page)
        => await context.Prescriptions
            .Include(p => p.MedicationList)
            .OrderByDescending(p => p.Date)
            .Skip((page - 1) * n)
            .Take(n)
            .ToListAsync();

    public async Task<List<PrescriptionItem>> GetItemsAsync(int prescriptionId)
        => await context.PrescriptionItems
            .Where(i => i.Prescription.PrescriptionId == prescriptionId)
            .ToListAsync();

    public async Task MarkPoliceNotifiedAsync(int patientId)
    {
        var records = await context.MedicalRecords
            .Where(r => r.MedicalHistory.Patient.PatientId == patientId)
            .ToListAsync();
        foreach (var record in records)
            record.PoliceNotified = true;
        await context.SaveChangesAsync();
    }

    public async Task<List<int>> GetPoliceNotifiedPatientIdsAsync(IEnumerable<int> patientIds)
        => await context.MedicalRecords
            .Where(r => patientIds.Contains(r.MedicalHistory.Patient.PatientId) && r.PoliceNotified)
            .Select(r => r.MedicalHistory.Patient.PatientId)
            .Distinct()
            .ToListAsync();

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
