using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services.PatientEr;

namespace Hospital.Services.PatientEr;

public class StatisticsService(
    IPatientRepository patientRepository,
    IMedicalRecordRepository recordRepository,
    IPrescriptionRepository prescriptionRepository) : Hospital.Shared.Services.IStatisticsService
{
    public async Task<Dictionary<string, int>> GetPatientsByBloodTypeAsync()
    {
        var patients = await patientRepository.GetAllAsync();
        return patients
            .Where(p => p.MedicalHistory?.BloodType.HasValue == true)
            .GroupBy(p => p.MedicalHistory!.BloodType!.Value.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetPatientsByRhAsync()
    {
        var patients = await patientRepository.GetAllAsync();
        return patients
            .Where(p => p.MedicalHistory?.Rh.HasValue == true)
            .GroupBy(p => p.MedicalHistory!.Rh!.Value.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetPatientGenderDistributionAsync()
    {
        var patients = await patientRepository.GetAllAsync();
        return patients
            .GroupBy(p => p.Sex.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetConsultationDistributionAsync()
    {
        var records = await recordRepository.GetAllAsync();
        return records
            .GroupBy(r => r.SourceType.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetTopDiagnosesAsync()
    {
        var records = await recordRepository.GetAllAsync();
        return records
            .Where(r => !string.IsNullOrWhiteSpace(r.Diagnosis))
            .GroupBy(r => r.Diagnosis!.Trim().ToUpperInvariant())
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetAgeDistributionAsync()
    {
        var patients = await patientRepository.GetAllAsync();
        var ageGroups = new Dictionary<string, int>
        {
            { "Pediatric (0-17)", 0 },
            { "Adult (18-64)", 0 },
            { "Geriatric (65+)", 0 },
        };

        foreach (Patient patient in patients)
        {
            int age = patient.GetAge();
            if (age <= 17) ageGroups["Pediatric (0-17)"]++;
            else if (age <= 64) ageGroups["Adult (18-64)"]++;
            else ageGroups["Geriatric (65+)"]++;
        }

        return ageGroups;
    }

    public async Task<Dictionary<string, int>> GetMostPrescribedMedsAsync()
    {
        var prescriptions = await prescriptionRepository.GetAllAsync();
        return prescriptions
            .Where(p => p.MedicationList is not null)
            .SelectMany(p => p.MedicationList)
            .Where(item => !string.IsNullOrWhiteSpace(item.MedicationName))
            .GroupBy(item => item.MedicationName.Trim().ToUpperInvariant())
            .OrderByDescending(g => g.Count())
            .Take(20)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetActiveVsArchivedRatioAsync()
    {
        var patients = await patientRepository.GetAllAsync();
        return new Dictionary<string, int>
        {
            { "Active", patients.Count(p => !p.IsArchived) },
            { "Archived", patients.Count(p => p.IsArchived) },
        };
    }
}
