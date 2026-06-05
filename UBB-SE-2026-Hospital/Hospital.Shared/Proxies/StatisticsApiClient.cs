using System.Net.Http.Json;
using System.Text.Json;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class StatisticsApiClient : ApiClientBase, IStatisticsApiClient, IStatisticsService
{
    private const string BaseUri = "api/statistics";
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions = new ()
    {
        PropertyNameCaseInsensitive = true,
    };

    public StatisticsApiClient(HttpClient httpClient) : base(httpClient)
    {
        this.httpClient = httpClient;
    }

    public Task<Dictionary<string, int>> GetActiveVsArchivedRatioAsync(CancellationToken cancellationToken)
    {
        return GetStatisticsAsync("active-vs-archived", cancellationToken);
    }

    public Task<Dictionary<string, int>> GetAgeDistributionAsync(CancellationToken cancellationToken)
    {
        return GetStatisticsAsync("age-distribution", cancellationToken);
    }

    public Task<Dictionary<string, int>> GetPatientGenderDistributionAsync(CancellationToken cancellationToken)
    {
        return GetStatisticsAsync("gender-distribution", cancellationToken);
    }

    public Task<Dictionary<string, int>> GetConsultationDistributionAsync(CancellationToken cancellationToken)
    {
        return GetStatisticsAsync("consultations", cancellationToken);
    }

    public Task<Dictionary<string, int>> GetTopDiagnosesAsync(CancellationToken cancellationToken)
    {
        return GetStatisticsAsync("top-diagnoses", cancellationToken);
    }

    public Task<Dictionary<string, int>> GetMostPrescribedMedsAsync(CancellationToken cancellationToken)
    {
        return GetStatisticsAsync("top-meds", cancellationToken);
    }

    private async Task<Dictionary<string, int>> GetStatisticsAsync(
        string route,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.GetAsync($"{BaseUri}/{route}", cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Dictionary<string, int>>(jsonOptions, cancellationToken) ?? new Dictionary<string, int>();
    }

    // IStatisticsService methods
    public async Task<Dictionary<string, int>> GetActiveVsArchivedRatioAsync()
        => await GetAsync<Dictionary<string, int>>($"{BaseUri}/active-vs-archived") ?? new Dictionary<string, int>();

    public async Task<Dictionary<string, int>> GetAgeDistributionAsync()
        => await GetAsync<Dictionary<string, int>>($"{BaseUri}/age-distribution") ?? new Dictionary<string, int>();

    public async Task<Dictionary<string, int>> GetPatientsByBloodTypeAsync()
        => await GetAsync<Dictionary<string, int>>($"{BaseUri}/blood-types") ?? new Dictionary<string, int>();

    public async Task<Dictionary<string, int>> GetPatientsByRhAsync()
        => await GetAsync<Dictionary<string, int>>($"{BaseUri}/rh-factor") ?? new Dictionary<string, int>();

    public async Task<Dictionary<string, int>> GetPatientGenderDistributionAsync()
        => await GetAsync<Dictionary<string, int>>($"{BaseUri}/gender-distribution") ?? new Dictionary<string, int>();

    public async Task<Dictionary<string, int>> GetConsultationDistributionAsync()
        => await GetAsync<Dictionary<string, int>>($"{BaseUri}/consultations") ?? new Dictionary<string, int>();

    public async Task<Dictionary<string, int>> GetTopDiagnosesAsync()
        => await GetAsync<Dictionary<string, int>>($"{BaseUri}/top-diagnoses") ?? new Dictionary<string, int>();

    public async Task<Dictionary<string, int>> GetMostPrescribedMedsAsync()
        => await GetAsync<Dictionary<string, int>>($"{BaseUri}/top-meds") ?? new Dictionary<string, int>();
}
