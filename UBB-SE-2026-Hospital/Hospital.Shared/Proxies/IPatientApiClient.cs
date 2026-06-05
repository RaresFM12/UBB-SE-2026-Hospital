using Hospital.Data.Models;
using Hospital.Data.Models;

namespace Hospital.Shared.Proxies;

public interface IPatientApiClient
{
    Task<Patient?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Patient> GetPatientDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<MedicalHistory?> GetMedicalHistoryAsync(int id, CancellationToken cancellationToken = default);
    Task<List<MedicalRecord>> GetMedicalRecordsAsync(int historyId, CancellationToken cancellationToken = default);
    Task<int> CreateMedicalRecordAsync(int patientId, CreateMedicalRecordRequest dto, CancellationToken cancellationToken = default);
    Task CreatePrescriptionForRecordAsync(int recordId, CreatePrescriptionRequest dto, CancellationToken cancellationToken = default);
    Task<List<string>> GetPatientAllergiesAsync(int id, CancellationToken cancellationToken = default);
    Task<Prescription?> GetPrescriptionByRecordIdAsync(int recordId, CancellationToken cancellationToken = default);
    Task<RecordExportDataDto> GetRecordExportDataAsync(int recordId, CancellationToken cancellationToken = default);
    Task<bool> IsHighRiskPatientAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string cnp, CancellationToken cancellationToken = default);
    Task<List<Patient>> SearchPatientsAsync(SearchPatientsRequest dto, CancellationToken cancellationToken = default);
    Task<Patient> CreatePatientAsync(CreatePatientRequest dto, CancellationToken cancellationToken = default);
    Task UpdatePatientAsync(int id, UpdatePatientRequest dto, CancellationToken cancellationToken = default);
    Task ArchivePatientAsync(int id, CancellationToken cancellationToken = default);
    Task DearchivePatientAsync(int id, CancellationToken cancellationToken = default);
    Task ArchiveAsDeceasedAsync(int id, ArchiveAsDeceasedRequest dto, CancellationToken cancellationToken = default);
    Task CreateMedicalHistoryAsync(int id, CreateMedicalHistoryRequest dto, CancellationToken cancellationToken = default);
    Task DeletePatientAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> IsHighRiskAsync(int id, CancellationToken cancellationToken = default);
}


