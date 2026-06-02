using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;

namespace Hospital.Shared.Services;

public interface IPatientService
{
    Task<IReadOnlyList<Patient>> GetPatientsAsync(CancellationToken cancellationToken = default);
    Task<Patient?> GetByIdAsync(int id);
    Task<Patient> GetPatientDetailsAsync(int id);
    Task<MedicalHistory?> GetMedicalHistoryAsync(int id);
    Task<List<MedicalRecord>> GetMedicalRecordsAsync(int historyId);
    Task<int> CreateMedicalRecordAsync(int patientId, CreateMedicalRecordRequest dto);
    Task<List<string>> GetPatientAllergiesAsync(int id);
    Task<Prescription?> GetPrescriptionByRecordIdAsync(int recordId);
    Task<bool> IsHighRiskPatientAsync(int id);
    Task<bool> ExistsAsync(string cnp);
    Task<List<Patient>> SearchPatientsAsync(SearchPatientsRequest dto);
    Task<Patient> CreatePatientAsync(CreatePatientRequest dto);
    Task UpdatePatientAsync(int id, UpdatePatientRequest dto);
    Task ArchivePatientAsync(int id);
    Task DearchivePatientAsync(int id);
    Task ArchiveAsDeceasedAsync(int id, ArchiveAsDeceasedRequest dto);
    Task CreateMedicalHistoryAsync(int id, CreateMedicalHistoryRequest dto);
    Task DeletePatientAsync(int id);
}
