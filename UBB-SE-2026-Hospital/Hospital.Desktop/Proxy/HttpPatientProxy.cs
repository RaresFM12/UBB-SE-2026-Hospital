using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Shared.Services;
using SharedPatient = Hospital.Shared.Models.PatientEr.Patient;
using SharedPrescription = Hospital.Shared.Models.PatientEr.Prescription;

namespace Hospital.Desktop.Proxy;

public class HttpPatientProxy(HttpClient httpClient) : ProxyBase(httpClient), IPatientService
{
    private const string BaseUri = "api/patients";

    public async Task<IReadOnlyList<Hospital.Data.Models.Patient>> GetPatientsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<Hospital.Data.Models.Patient>>(BaseUri) ?? [];

    public async Task<Patient?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => await GetAsync<Patient>($"{BaseUri}/{id}");

    public async Task<List<Patient>> SearchPatientsAsync(SearchPatientsRequest? searchCriteria, CancellationToken cancellationToken)
        => await PostAsync<SearchPatientsRequest?, List<Patient>>($"{BaseUri}/search", searchCriteria) ?? [];

    public async Task<Patient> CreatePatientAsync(CreatePatientRequest request, CancellationToken cancellationToken = default)
        => await PostAsync<CreatePatientRequest, Patient>(BaseUri, request)
           ?? throw new InvalidOperationException("Failed to create patient: no response from server.");

    public async Task CreateMedicalHistoryAsync(int patientId, CreateMedicalHistoryRequest request, CancellationToken cancellationToken = default)
        => await PostAsync<CreateMedicalHistoryRequest, object>($"{BaseUri}/{patientId}/medical-history", request);

    public async Task<Hospital.Data.Models.Patient> GetPatientDetailsAsync(int patientId, CancellationToken cancellationToken = default)
        => await GetAsync<Hospital.Data.Models.Patient>($"{BaseUri}/{patientId}/details")
           ?? throw new KeyNotFoundException($"Patient with ID {patientId} not found.");

    public async Task<Hospital.Data.Models.Prescription?> GetPrescriptionByRecordIdAsync(int recordId, CancellationToken cancellationToken = default)
        => await GetAsync<Hospital.Data.Models.Prescription>($"{BaseUri}/records/{recordId}/prescription");

    public async Task<List<string>> GetPatientAllergiesAsync(int patientId, CancellationToken cancellationToken = default)
        => await GetAsync<List<string>>($"{BaseUri}/{patientId}/allergies") ?? [];

    public async Task<bool> IsHighRiskPatientAsync(int patientId, CancellationToken cancellationToken = default)
        => await GetAsync<bool>($"{BaseUri}/{patientId}/high-risk");

    public async Task<RecordExportDataDto> GetRecordExportDataAsync(int recordId, CancellationToken cancellationToken = default)
        => await GetAsync<RecordExportDataDto>($"{BaseUri}/records/{recordId}/export")
           ?? throw new KeyNotFoundException($"Record export data for ID {recordId} not found.");

    public async Task UpdatePatientAsync(Patient patient, CancellationToken cancellationToken = default)
        => await PutAsync($"{BaseUri}/{patient.PatientId}", patient);

    public async Task ArchivePatientAsync(int patientId, CancellationToken cancellationToken = default)
        => await PutAsync<object>($"{BaseUri}/{patientId}/archive", new { });

    public async Task DearchivePatientAsync(int patientId, CancellationToken cancellationToken = default)
        => await PutAsync<object>($"{BaseUri}/{patientId}/dearchive", new { });

    public async Task ArchiveAsDeceasedAsync(int patientId, DateTime deathDate, CancellationToken cancellationToken = default)
        => await PutAsync<object>($"{BaseUri}/{patientId}/archive-deceased", new { deathDate });

    public async Task<int> CreateMedicalRecordAsync(int patientId, MedicalRecord record, CancellationToken cancellationToken = default)
        => await PostAsync<MedicalRecord, int>($"{BaseUri}/{patientId}/medical-records", record);

    public async Task CreatePrescriptionAsync(int recordId, Hospital.Data.Models.Prescription prescription)
        => await PostAsync<Hospital.Data.Models.Prescription, object>($"{BaseUri}/records/{recordId}/prescription", prescription);

    // Extra helpers not in interface
    public async Task<MedicalHistory?> GetMedicalHistoryAsync(int id)
        => await GetAsync<MedicalHistory>($"{BaseUri}/{id}/medical-history");

    public async Task<bool> ExistsAsync(string cnp)
        => await GetAsync<bool>($"{BaseUri}/exists/{cnp}");

    public async Task DeletePatientAsync(int id)
        => await DeleteAsync($"{BaseUri}/{id}");
    public async Task UpdatePatientAsync(int patientId, Hospital.Data.Models.DTOs.UpdatePatientRequest request, CancellationToken cancellationToken = default)
        => await PutAsync($"{BaseUri}/{patientId}", request);
    public async Task<List<Hospital.Data.Models.MedicalRecord>> GetMedicalRecordsAsync(int historyId)
        => await GetAsync<List<Hospital.Data.Models.MedicalRecord>>($"api/patients/{historyId}/medical-records") ?? [];

}
