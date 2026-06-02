using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Proxy;

public class HttpPatientProxy(HttpClient httpClient) : ProxyBase(httpClient), IPatientService
{
    private const string BaseUri = "api/patients";

    public async Task<IReadOnlyList<Patient>> GetPatientsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<Patient>>(BaseUri) ?? [];

    public async Task<Patient?> GetByIdAsync(int id)
        => await GetAsync<Patient>($"{BaseUri}/{id}");

    public async Task<Patient> GetPatientDetailsAsync(int id)
        => await GetAsync<Patient>($"{BaseUri}/{id}/details")
           ?? throw new KeyNotFoundException($"Patient with ID {id} not found.");

    public async Task<MedicalHistory?> GetMedicalHistoryAsync(int id)
        => await GetAsync<MedicalHistory>($"{BaseUri}/{id}/medical-history");

    public async Task<List<MedicalRecord>> GetMedicalRecordsAsync(int historyId)
        => await GetAsync<List<MedicalRecord>>($"{BaseUri}/{historyId}/medical-records") ?? [];

    public async Task<int> CreateMedicalRecordAsync(int patientId, CreateMedicalRecordRequest dto)
        => await PostAsync<CreateMedicalRecordRequest, int>($"{BaseUri}/{patientId}/medical-records", dto);

    public async Task<List<string>> GetPatientAllergiesAsync(int id)
        => await GetAsync<List<string>>($"{BaseUri}/{id}/allergies") ?? [];

    public async Task<Prescription?> GetPrescriptionByRecordIdAsync(int recordId)
        => await GetAsync<Prescription>($"{BaseUri}/records/{recordId}/prescription");

    public async Task<bool> IsHighRiskPatientAsync(int id)
        => await GetAsync<bool>($"{BaseUri}/{id}/high-risk");

    public async Task<bool> ExistsAsync(string cnp)
        => await GetAsync<bool>($"{BaseUri}/exists/{cnp}");

    public async Task<List<Patient>> SearchPatientsAsync(SearchPatientsRequest dto)
        => await PostAsync<SearchPatientsRequest, List<Patient>>($"{BaseUri}/search", dto) ?? [];

    public async Task<Patient> CreatePatientAsync(CreatePatientRequest dto)
        => await PostAsync<CreatePatientRequest, Patient>(BaseUri, dto)
           ?? throw new InvalidOperationException("Failed to create patient: no response from server.");

    public async Task UpdatePatientAsync(int id, UpdatePatientRequest dto)
        => await PutAsync($"{BaseUri}/{id}", dto);

    public async Task ArchivePatientAsync(int id)
        => await PutAsync<object>($"{BaseUri}/{id}/archive", new { });

    public async Task DearchivePatientAsync(int id)
        => await PutAsync<object>($"{BaseUri}/{id}/dearchive", new { });

    public async Task ArchiveAsDeceasedAsync(int id, ArchiveAsDeceasedRequest dto)
        => await PutAsync($"{BaseUri}/{id}/archive-deceased", dto);

    public async Task CreateMedicalHistoryAsync(int id, CreateMedicalHistoryRequest dto)
        => await PostAsync<CreateMedicalHistoryRequest, object>($"{BaseUri}/{id}/medical-history", dto);

    public async Task DeletePatientAsync(int id)
        => await DeleteAsync($"{BaseUri}/{id}");
}
