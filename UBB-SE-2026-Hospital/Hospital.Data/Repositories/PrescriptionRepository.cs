using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class PrescriptionRepository(HospitalDbContext context) : IPrescriptionRepository
{
    public async Task<Prescription?> GetByIdAsync(int prescriptionId)
        => await context.Prescriptions
            .Include(patient => patient.MedicationList)
            .FirstOrDefaultAsync(patient => patient.PrescriptionId == prescriptionId);

    public async Task<List<Prescription>> GetAllAsync()
        => await context.Prescriptions
            .Include(patient => patient.MedicationList)
            .ToListAsync();

    public async Task<List<Prescription>> GetFilteredAsync(PrescriptionFilter filter)
    {
        var query = context.Prescriptions
            .Include(patient => patient.MedicationList)
            .Include(patient => patient.MedicalRecord)
                .ThenInclude(medicalRecord => medicalRecord.MedicalHistory)
                    .ThenInclude(mh => mh.Patient)
            .Include(patient => patient.MedicalRecord)
                .ThenInclude(medicalRecord => medicalRecord.StaffMember)
            .AsQueryable();

        if (filter.PrescriptionId.HasValue)
            query = query.Where(patient => patient.PrescriptionId == filter.PrescriptionId.Value);

        if (filter.DateFrom.HasValue)
            query = query.Where(patient => patient.Date >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(patient => patient.Date <= filter.DateTo.Value);

        if (!string.IsNullOrWhiteSpace(filter.DoctorName))
            query = query.Where(patient =>
                patient.MedicalRecord.StaffMember.FirstName.Contains(filter.DoctorName) ||
                patient.MedicalRecord.StaffMember.LastName.Contains(filter.DoctorName));

        if (!string.IsNullOrWhiteSpace(filter.PatientName))
            query = query.Where(patient =>
                patient.MedicalRecord.MedicalHistory.Patient.FirstName.Contains(filter.PatientName) ||
                patient.MedicalRecord.MedicalHistory.Patient.LastName.Contains(filter.PatientName));

        if (!string.IsNullOrWhiteSpace(filter.MedicationName))
            query = query.Where(patient =>
                patient.MedicationList.Any(i => i.MedicationName.Contains(filter.MedicationName)));

        return await query.ToListAsync();
    }

    public async Task<List<Prescription>> GetByRecordIdAsync(int recordId)
        => await context.Prescriptions
            .Include(patient => patient.MedicationList)
            .Where(patient => patient.MedicalRecord.RecordId == recordId)
            .ToListAsync();

    public async Task<List<Prescription>> GetPotentialDrugAddictsAsync()
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);
        var prescriptions = await context.Prescriptions
            .Include(patient => patient.MedicationList)
            .Include(patient => patient.MedicalRecord)
                .ThenInclude(medicalRecord => medicalRecord.MedicalHistory)
                    .ThenInclude(mh => mh.Patient)
            .Where(patient => patient.Date >= cutoff)
            .ToListAsync();

        return prescriptions
            .GroupBy(patient => patient.MedicalRecord?.RecordId)
            .Where(g => g.Count() >= 5)
            .SelectMany(g => g)
            .ToList();
    }

    public async Task<List<Prescription>> GetTopNAsync(int n, int page)
        => await context.Prescriptions
            .Include(patient => patient.MedicationList)
            .Include(patient => patient.MedicalRecord)
                .ThenInclude(medicalRecord => medicalRecord.MedicalHistory)
                    .ThenInclude(mh => mh.Patient)
            .Include(patient => patient.MedicalRecord)
                .ThenInclude(medicalRecord => medicalRecord.StaffMember)
            .OrderByDescending(patient => patient.Date)
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
            .Where(medicalRecord => medicalRecord.MedicalHistory.Patient.PatientId == patientId)
            .ToListAsync();
        foreach (var record in records)
            record.PoliceNotified = true;
        await context.SaveChangesAsync();
    }

    public async Task<List<int>> GetPoliceNotifiedPatientIdsAsync(IEnumerable<int> patientIds)
        => await context.MedicalRecords
            .Where(medicalRecord => patientIds.Contains(medicalRecord.MedicalHistory.Patient.PatientId) && medicalRecord.PoliceNotified)
            .Select(medicalRecord => medicalRecord.MedicalHistory.Patient.PatientId)
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
