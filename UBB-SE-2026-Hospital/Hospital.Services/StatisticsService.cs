using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;

namespace Hospital.Services;

public class StatisticsService(
    IPatientRepository patientRepository,
    IMedicalRecordRepository recordRepository,
    IPrescriptionRepository prescriptionRepository) : Hospital.Shared.Services.IStatisticsService
{
    public async Task<Dictionary<string, int>> GetPatientsByBloodTypeAsync()
    {
        var patients = await patientRepository.GetAllAsync();
        return patients
            .Where(patient => patient.MedicalHistory?.BloodType.HasValue == true)
            .GroupBy(patient => patient.MedicalHistory!.BloodType!.Value.ToString())
            .ToDictionary(grouping => grouping.Key, grouping => grouping.Count());
    }

    public async Task<Dictionary<string, int>> GetPatientsByRhAsync()
    {
        var patients = await patientRepository.GetAllAsync();
        return patients
            .Where(patient => patient.MedicalHistory?.Rh.HasValue == true)
            .GroupBy(patient => patient.MedicalHistory!.Rh!.Value.ToString())
            .ToDictionary(grouping => grouping.Key, grouping => grouping.Count());
    }

    public async Task<Dictionary<string, int>> GetPatientGenderDistributionAsync()
    {
        var patients = await patientRepository.GetAllAsync();
        return patients
            .GroupBy(patient => patient.Sex.ToString())
            .ToDictionary(grouping => grouping.Key, grouping => grouping.Count());
    }

    public async Task<Dictionary<string, int>> GetConsultationDistributionAsync()
    {
        var records = await recordRepository.GetAllAsync();
        return records
            .GroupBy(record => record.SourceType.ToString())
            .ToDictionary(grouping => grouping.Key, grouping => grouping.Count());
    }

    public async Task<Dictionary<string, int>> GetTopDiagnosesAsync()
    {
        var records = await recordRepository.GetAllAsync();
        return records
            .Where(record => !string.IsNullOrWhiteSpace(record.Diagnosis))
            .GroupBy(record => record.Diagnosis!.Trim().ToUpperInvariant())
            .ToDictionary(grouping => grouping.Key, grouping => grouping.Count());
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
            .Where(patient => patient.MedicationList is not null)
            .SelectMany(patient => patient.MedicationList)
            .Where(item => !string.IsNullOrWhiteSpace(item.MedicationName))
            .GroupBy(item => item.MedicationName.Trim().ToUpperInvariant())
            .OrderByDescending(grouping => grouping.Count())
            .Take(20)
            .ToDictionary(grouping => grouping.Key, grouping => grouping.Count());
    }

    public async Task<Dictionary<string, int>> GetActiveVsArchivedRatioAsync()
    {
        var patients = await patientRepository.GetAllAsync();
        return new Dictionary<string, int>
        {
            { "Active", patients.Count(patient => !patient.IsArchived) },
            { "Archived", patients.Count(patient => patient.IsArchived) },
        };
    }
}
