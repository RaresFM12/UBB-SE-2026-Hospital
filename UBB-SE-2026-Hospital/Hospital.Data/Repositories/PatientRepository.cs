using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class PatientRepository(HospitalDbContext context) : IPatientRepository
{
    public async Task<Patient?> GetByIdAsync(int patientId)
    {
        return await context.Patients
            .Include(patient => patient.MedicalHistory)
                .ThenInclude(medicalHistory => medicalHistory!.MedicalRecords)
                    .ThenInclude(record => record.Prescription)
                        .ThenInclude(prescription => prescription!.MedicationList)
            .Include(patient => patient.MedicalHistory)
                .ThenInclude(medicalHistory => medicalHistory!.PatientAllergies)
                    .ThenInclude(patientAllergy => patientAllergy.Allergy)
            .FirstOrDefaultAsync(patient => patient.PatientId == patientId);
    }

    public async Task<List<Patient>> GetAllAsync()
        => await context.Patients
            .Include(patient => patient.MedicalHistory)
                .ThenInclude(medicalHistory => medicalHistory!.PatientAllergies)
                    .ThenInclude(patientAllergy => patientAllergy.Allergy)
            .ToListAsync();

    public async Task<List<Patient>> GetFilteredAsync(PatientFilter filter)
    {
        var query = context.Patients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.CNP))
            query = query.Where(patient => patient.Cnp == filter.CNP);

        if (!string.IsNullOrWhiteSpace(filter.NamePart))
            query = query.Where(patient => (patient.FirstName + " " + patient.LastName).Contains(filter.NamePart));

        if (filter.Sex.HasValue)
            query = query.Where(patient => patient.Sex == filter.Sex.Value);

        if (filter.MinAge.HasValue)
        {
            var maxBirth = DateTime.Today.AddYears(-filter.MinAge.Value);
            query = query.Where(patient => patient.DateOfBirth <= maxBirth);
        }

        if (filter.MaxAge.HasValue)
        {
            var minBirth = DateTime.Today.AddYears(-filter.MaxAge.Value - 1);
            query = query.Where(patient => patient.DateOfBirth >= minBirth);
        }

        return await query.ToListAsync();
    }

    public async Task<Patient> CreateAsync(Patient patient)
    {
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        return patient;
    }

    public async Task<Patient> UpdateAsync(Patient patient)
    {
        context.Patients.Update(patient);
        await context.SaveChangesAsync();
        return patient;
    }

    public async Task DeleteAsync(int patientId)
    {
        var patient = await context.Patients.FindAsync(patientId);
        if (patient is not null)
        {
            context.Patients.Remove(patient);
            await context.SaveChangesAsync();
        }
    }
}
