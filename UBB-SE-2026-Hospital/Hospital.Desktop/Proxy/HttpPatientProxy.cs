using System.Net.Http.Json;
using Hospital.Shared.Models.PatientEr;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Proxy;

public class HttpPatientProxy(HttpClient httpClient) : IPatientService
{
    public async Task<IReadOnlyList<Patient>> GetPatientsAsync(CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<List<Patient>>("api/patients", cancellationToken) ?? [];
}
