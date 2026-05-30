using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class PatientRepository(HospitalDbContext context) : IPatientRepository
{
    public async Task<Patient?> GetByIdAsync(int patientId)
        => await context.Patients.FindAsync(patientId);

    public async Task<List<Patient>> GetAllAsync()
        => await context.Patients.ToListAsync();

    public async Task<List<Patient>> GetFilteredAsync(PatientFilter filter)
    {
        var all = await context.Patients.ToListAsync();
        IEnumerable<Patient> query = all;

        bool MatchesCnp(Patient patient) => patient.Cnp == filter.CNP;
        bool MatchesName(Patient patient) => (patient.FirstName + " " + patient.LastName).Contains(filter.NamePart!);
        bool MatchesSex(Patient patient) => patient.Sex == filter.Sex!.Value;

        if (!string.IsNullOrWhiteSpace(filter.CNP))
            query = query.Where(MatchesCnp);

        if (!string.IsNullOrWhiteSpace(filter.NamePart))
            query = query.Where(MatchesName);

        if (filter.Sex.HasValue)
            query = query.Where(MatchesSex);

        if (filter.MinAge.HasValue)
        {
            var maxBirth = DateTime.Today.AddYears(-filter.MinAge.Value);
            bool BornBeforeMax(Patient patient) => patient.DateOfBirth <= maxBirth;
            query = query.Where(BornBeforeMax);
        }

        if (filter.MaxAge.HasValue)
        {
            var minBirth = DateTime.Today.AddYears(-filter.MaxAge.Value - 1);
            bool BornAfterMin(Patient patient) => patient.DateOfBirth >= minBirth;
            query = query.Where(BornAfterMin);
        }

        return query.ToList();
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
