using Hospital.Data.Models;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Proxy;

public class HttpAddictDetectionProxy(HttpClient httpClient) : ProxyBase(httpClient), IAddictDetectionService
{
    private const string BaseUri = "api/addicts";

    public async Task<List<Patient>> GetAddictCandidatesAsync()
        => await GetAsync<List<Patient>>($"{BaseUri}/candidates") ?? [];

    public async Task<string> BuildPoliceReportAsync(int patientId)
        => await PostAsync<BuildPoliceReportRequest, string>($"{BaseUri}/police-report", new BuildPoliceReportRequest { PatientId = patientId }) ?? string.Empty;

    public async Task<string> GetChronicConditionsAsync(int patientId)
        => await GetAsync<string>($"{BaseUri}/{patientId}/chronic-conditions") ?? "None reported.";
}
