using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class AddictDetectionApiClient : ApiClientBase, IAddictDetectionApiClient, IAddictDetectionService
{
    private const string BaseUri = "api/addicts";

    public AddictDetectionApiClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<List<Patient>> GetCandidatesAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<Patient>>($"{BaseUri}/candidates", cancellationToken) ?? [];

    public async Task<string> BuildPoliceReportAsync(int patientId, CancellationToken cancellationToken = default)
        => await PostAsync<BuildPoliceReportRequest, string>($"{BaseUri}/police-report", new BuildPoliceReportRequest { PatientId = patientId }, cancellationToken) ?? string.Empty;

    public async Task MarkPoliceNotifiedAsync(int patientId, CancellationToken cancellationToken = default)
        => await PostAsync<object?>($"{BaseUri}/{patientId}/notify", null, cancellationToken);

    // IAddictDetectionService methods
    public async Task<List<Patient>> GetAddictCandidatesAsync()
        => await GetAsync<List<Patient>>($"{BaseUri}/candidates") ?? [];

    public async Task<string> BuildPoliceReportAsync(int patientId)
        => await PostAsync<BuildPoliceReportRequest, string>($"{BaseUri}/police-report", new BuildPoliceReportRequest { PatientId = patientId }) ?? string.Empty;

    public async Task<string> GetChronicConditionsAsync(int patientId)
        => await GetAsync<string>($"{BaseUri}/{patientId}/chronic-conditions") ?? "None reported.";

    public Task MarkPoliceNotifiedAsync(int patientId)
        => MarkPoliceNotifiedAsync(patientId, CancellationToken.None);
}

