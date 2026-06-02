using Hospital.Data.Models.DTOs;
using Hospital.Shared.DTOs;
using Hospital.Shared.Models.PatientEr;
using Hospital.Shared.Services;
using System.Net.Http.Json;

namespace Hospital.Desktop.Proxy;

public class HttpPatientProxy(HttpClient httpClient) : IPatientService
{
    public async Task<List<Data.Models.Patient>> SearchPatientsAsync(SearchPatientsRequest? searchCriteria, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("api/patients/search", searchCriteria, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Data.Models.Patient>>(cancellationToken: cancellationToken) ?? [];
    }

    public async Task<Data.Models.Patient?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => await httpClient.GetFromJsonAsync<Data.Models.Patient>($"api/patients/{id}", cancellationToken);

    public async Task<IReadOnlyList<Patient>> GetPatientsAsync(CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<List<Patient>>("api/patients", cancellationToken) ?? [];

    public async Task<Patient> GetPatientDetailsAsync(int patientId, CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<Patient>($"api/patients/{patientId}", cancellationToken) ?? throw new Exception("Patient not found");

    public async Task<Prescription?> GetPrescriptionByRecordIdAsync(int recordId, CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<Prescription>($"api/records/{recordId}/prescription", cancellationToken);

    public async Task<List<string>> GetPatientAllergiesAsync(int patientId, CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<List<string>>($"api/patients/{patientId}/allergies", cancellationToken) ?? [];

    public async Task<bool> IsHighRiskPatientAsync(int patientId, CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<bool>($"api/patients/{patientId}/high-risk", cancellationToken);

    public async Task<RecordExportDataDto> GetRecordExportDataAsync(int recordId, CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<RecordExportDataDto>($"api/records/{recordId}/export", cancellationToken) ?? throw new Exception("Export data not found");

    public async Task<int> CreateMedicalRecordAsync(int patientId, Data.Models.MedicalRecord record, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"api/patients/{patientId}/records", record, cancellationToken);
        return await response.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
    }

    public async Task CreatePrescriptionAsync(int recordId, Prescription prescription)
    {
        await httpClient.PostAsJsonAsync($"api/records/{recordId}/prescription", prescription);
    }
}
