using System.Net;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class PatientApiClient : ApiClientBase, IPatientApiClient, IPatientService
{
    private const string BaseUri = "api/patients";

    public PatientApiClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<Patient?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<Patient>($"{BaseUri}/{id}", cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    public async Task<Patient> GetPatientDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        await GetAsync<Patient>($"{BaseUri}/{id}/details", cancellationToken)
        ?? throw new KeyNotFoundException($"Patient with ID {id} not found.");

    public Task<MedicalHistory?> GetMedicalHistoryAsync(int id, CancellationToken cancellationToken = default) =>
        GetAsync<MedicalHistory>($"{BaseUri}/{id}/medical-history", cancellationToken);

    public async Task<List<MedicalRecord>> GetMedicalRecordsAsync(int historyId, CancellationToken cancellationToken = default) =>
        await GetAsync<List<MedicalRecord>>($"{BaseUri}/{historyId}/medical-records", cancellationToken) ?? new List<MedicalRecord>();

    public async Task<int> CreateMedicalRecordAsync(
        int patientId,
        CreateMedicalRecordRequest dto,
        CancellationToken cancellationToken = default) =>
        await PostAsync<CreateMedicalRecordRequest, int>($"{BaseUri}/{patientId}/medical-records", dto, cancellationToken);

    public Task CreatePrescriptionForRecordAsync(
        int recordId,
        CreatePrescriptionRequest dto,
        CancellationToken cancellationToken = default) =>
        PostAsync($"{BaseUri}/records/{recordId}/prescription", dto, cancellationToken);

    public async Task<List<string>> GetPatientAllergiesAsync(int id, CancellationToken cancellationToken = default) =>
        await GetAsync<List<string>>($"{BaseUri}/{id}/allergies", cancellationToken) ?? new List<string>();

    public async Task<bool> IsHighRiskPatientAsync(int id, CancellationToken cancellationToken = default) =>
        await GetAsync<bool>($"{BaseUri}/{id}/high-risk", cancellationToken);

    public async Task<bool> ExistsAsync(string cnp, CancellationToken cancellationToken = default) =>
        await GetAsync<bool>($"{BaseUri}/exists/{cnp}", cancellationToken);

    public async Task<List<Patient>> SearchPatientsAsync(
        SearchPatientsRequest dto,
        CancellationToken cancellationToken = default) =>
        await PostAsync<SearchPatientsRequest, List<Patient>>($"{BaseUri}/search", dto, cancellationToken) ?? new List<Patient>();

    public async Task<Patient> CreatePatientAsync(
        CreatePatientRequest dto,
        CancellationToken cancellationToken = default) =>
        await PostAsync<CreatePatientRequest, Patient>(BaseUri, dto, cancellationToken)
        ?? throw new InvalidOperationException("Failed to create patient: no response from server.");

    public Task UpdatePatientAsync(int id, UpdatePatientRequest dto, CancellationToken cancellationToken = default) =>
        PutAsync($"{BaseUri}/{id}", dto, cancellationToken);

    public Task ArchivePatientAsync(int id, CancellationToken cancellationToken = default) =>
        PutAsync<object>($"{BaseUri}/{id}/archive", new { }, cancellationToken);

    public Task DearchivePatientAsync(int id, CancellationToken cancellationToken = default) =>
        PutAsync<object>($"{BaseUri}/{id}/dearchive", new { }, cancellationToken);

    public Task ArchiveAsDeceasedAsync(
        int id,
        ArchiveAsDeceasedRequest dto,
        CancellationToken cancellationToken = default) =>
        PutAsync($"{BaseUri}/{id}/archive-deceased", dto, cancellationToken);

    public Task CreateMedicalHistoryAsync(
        int id,
        CreateMedicalHistoryRequest dto,
        CancellationToken cancellationToken = default) =>
        PostAsync($"{BaseUri}/{id}/medical-history", dto, cancellationToken);

    public Task DeletePatientAsync(int id, CancellationToken cancellationToken = default) =>
        DeleteAsync($"{BaseUri}/{id}", cancellationToken);

    public async Task<bool> IsHighRiskAsync(int id, CancellationToken cancellationToken = default) =>
        await GetAsync<bool>($"{BaseUri}/{id}/high-risk", cancellationToken);

    public async Task<RecordExportDataDto> GetRecordExportDataAsync(
        int recordId,
        CancellationToken cancellationToken = default) =>
        await GetAsync<RecordExportDataDto>($"{BaseUri}/records/{recordId}/export-data", cancellationToken)
        ?? throw new KeyNotFoundException($"Medical record {recordId} not found.");

    public async Task<Prescription?> GetPrescriptionByRecordIdAsync(
        int recordId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<Prescription>($"{BaseUri}/records/{recordId}/prescription", cancellationToken);
        }
        catch (InvalidOperationException e) when (e.Message.Contains(((int)HttpStatusCode.NotFound).ToString(), StringComparison.Ordinal))
        {
            return null;
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    // IPatientService implementation
    public async Task<IReadOnlyList<Patient>> GetPatientsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<Patient>>(BaseUri, cancellationToken) ?? [];

    public async Task UpdatePatientAsync(Patient patient, CancellationToken cancellationToken = default)
        => await PutAsync($"{BaseUri}/{patient.PatientId}", patient, cancellationToken);

    public async Task ArchiveAsDeceasedAsync(int patientId, DateTime deathDate, CancellationToken cancellationToken = default)
        => await PutAsync<object>($"{BaseUri}/{patientId}/archive-deceased", new { deathDate }, cancellationToken);

    public async Task<int> CreateMedicalRecordAsync(int patientId, MedicalRecord record, CancellationToken cancellationToken = default)
        => await PostAsync<MedicalRecord, int>($"{BaseUri}/{patientId}/medical-records", record, cancellationToken);

    public async Task CreatePrescriptionAsync(int recordId, Prescription prescription)
        => await PostAsync<Prescription, object>($"{BaseUri}/records/{recordId}/prescription", prescription);

    // Some extra getters
    public async Task<MedicalHistory?> GetMedicalHistoryAsync(int id)
        => await GetAsync<MedicalHistory>($"{BaseUri}/{id}/medical-history");

    public async Task<bool> ExistsAsync(string cnp)
        => await GetAsync<bool>($"{BaseUri}/exists/{cnp}");

    public async Task DeletePatientAsync(int id)
        => await DeleteAsync($"{BaseUri}/{id}");

    public async Task<List<MedicalRecord>> GetMedicalRecordsAsync(int historyId)
        => await GetAsync<List<MedicalRecord>>($"api/patients/{historyId}/medical-records") ?? [];
}

