using Hospital.Data.Models; 
using DbPatient = Hospital.Data.Models.Patient;
using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IPatientService
{
    Task<List<Data.Models.Patient>> SearchPatientsAsync(SearchPatientsRequest? searchCriteria, CancellationToken cancellationToken = default);
    Task<Data.Models.Patient?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<DbPatient>> GetPatientsAsync(CancellationToken cancellationToken = default);
    Task<DbPatient> CreatePatientAsync(CreatePatientRequest request, CancellationToken cancellationToken = default);
    Task CreateMedicalHistoryAsync(int patientId, CreateMedicalHistoryRequest request, CancellationToken cancellationToken = default);
    Task<DbPatient> GetPatientDetailsAsync(int patientId, CancellationToken cancellationToken = default);
    Task<Hospital.Data.Models.Prescription?> GetPrescriptionByRecordIdAsync(int recordId, CancellationToken cancellationToken = default);
    Task<List<string>> GetPatientAllergiesAsync(int patientId, CancellationToken cancellationToken = default);
    Task<bool> IsHighRiskPatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task<RecordExportDataDto> GetRecordExportDataAsync(int recordId, CancellationToken cancellationToken = default);
    Task UpdatePatientAsync(DbPatient patient, CancellationToken cancellationToken = default);
    Task ArchivePatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task DearchivePatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task ArchiveAsDeceasedAsync(int patientId, DateTime deathDate, CancellationToken cancellationToken = default);
    Task<int> CreateMedicalRecordAsync(int patientId, Data.Models.MedicalRecord record, CancellationToken cancellationToken = default);
    Task CreatePrescriptionAsync(int recordId, Hospital.Data.Models.Prescription prescription);
    Task UpdatePatientAsync(int patientId, UpdatePatientRequest request, CancellationToken cancellationToken = default);
    Task<Hospital.Data.Models.MedicalHistory?> GetMedicalHistoryAsync(int patientId);
    Task<List<Hospital.Data.Models.MedicalRecord>> GetMedicalRecordsAsync(int historyId);
    Task<bool> ExistsAsync(string cnp);
}
