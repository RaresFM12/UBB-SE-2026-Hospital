using Hospital.Data.Models.DTOs; 
using Hospital.Shared.Models.PatientEr;

namespace Hospital.Shared.Services;

public interface IPatientService
{
    Task<List<Data.Models.Patient>> SearchPatientsAsync(SearchPatientsRequest? searchCriteria, CancellationToken cancellationToken);
    Task<Data.Models.Patient?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Patient>> GetPatientsAsync(CancellationToken cancellationToken = default);
    Task<Patient> GetPatientDetailsAsync(int patientId, CancellationToken cancellationToken = default);
    Task<Prescription?> GetPrescriptionByRecordIdAsync(int recordId, CancellationToken cancellationToken = default);
    Task<List<string>> GetPatientAllergiesAsync(int patientId, CancellationToken cancellationToken = default);
    Task<bool> IsHighRiskPatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task<RecordExportDataDto> GetRecordExportDataAsync(int recordId, CancellationToken cancellationToken = default);
    Task<int> CreateMedicalRecordAsync(int patientId, Data.Models.MedicalRecord record, CancellationToken cancellationToken = default);
    Task CreatePrescriptionAsync(int recordId, Prescription prescription);
}