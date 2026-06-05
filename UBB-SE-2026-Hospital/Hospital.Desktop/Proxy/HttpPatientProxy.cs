using Hospital.Data.Models.DTOs;
using Hospital.Shared.DTOs;
using Hospital.Shared.Models.PatientEr;
using Hospital.Shared.Services;
using System.Net.Http.Json;
using DbPatient = Hospital.Data.Models.Patient;

namespace Hospital.Desktop.Proxy;

public class HttpPatientProxy(HttpClient httpClient) : IPatientService
{
    public async Task<List<DbPatient>> SearchPatientsAsync(SearchPatientsRequest? searchCriteria, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("api/patients/search", searchCriteria, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<DbPatient>>(cancellationToken: cancellationToken) ?? [];
    }

    public async Task<DbPatient?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => await httpClient.GetFromJsonAsync<DbPatient>($"api/patients/{id}", cancellationToken);

    public async Task<IReadOnlyList<Patient>> GetPatientsAsync(CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<List<Patient>>("api/patients", cancellationToken) ?? [];

    public async Task<DbPatient> CreatePatientAsync(CreatePatientRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/patients", request, cancellationToken);
        return await response.Content.ReadFromJsonAsync<DbPatient>(cancellationToken: cancellationToken)
            ?? throw new Exception("Patient was not created.");
    }

    public async Task CreateMedicalHistoryAsync(int patientId, CreateMedicalHistoryRequest request, CancellationToken cancellationToken = default)
    {
        _ = await httpClient.PostAsJsonAsync($"api/patients/{patientId}/medical-history", request, cancellationToken);
    }

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

    public async Task UpdatePatientAsync(DbPatient patient, CancellationToken cancellationToken = default)
    {
        _ = await httpClient.PutAsJsonAsync($"api/patients/{patient.PatientId}", patient, cancellationToken);
    }

    public async Task ArchivePatientAsync(int patientId, CancellationToken cancellationToken = default)
    {
        _ = await httpClient.PutAsJsonAsync($"api/patients/{patientId}/archive", new { }, cancellationToken);
    }

    public async Task DearchivePatientAsync(int patientId, CancellationToken cancellationToken = default)
    {
        _ = await httpClient.PutAsJsonAsync($"api/patients/{patientId}/dearchive", new { }, cancellationToken);
    }

    public async Task ArchiveAsDeceasedAsync(int patientId, DateTime deathDate, CancellationToken cancellationToken = default)
    {
        _ = await httpClient.PutAsJsonAsync($"api/patients/{patientId}/archive-deceased", new { DeathDate = deathDate }, cancellationToken);
    }

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
