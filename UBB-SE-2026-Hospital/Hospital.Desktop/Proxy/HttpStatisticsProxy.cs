using Hospital.Shared.Services;
using System.Collections.Generic;

namespace Hospital.Desktop.Proxy;

public class HttpStatisticsProxy(HttpClient httpClient) : ProxyBase(httpClient), IStatisticsService
{
    private const string BaseUri = "api/statistics";

    public async Task<Dictionary<string, int>> GetActiveVsArchivedRatioAsync()
        => await GetAsync<Dictionary<string, int>>($"{BaseUri}/active-vs-archived") ?? [];

    public async Task<Dictionary<string, int>> GetAgeDistributionAsync()
        => await GetAsync<Dictionary<string, int>>($"{BaseUri}/age-distribution") ?? [];

    public async Task<Dictionary<string, int>> GetPatientsByBloodTypeAsync()
        => await GetAsync<Dictionary<string, int>>($"{BaseUri}/blood-types") ?? [];

    public async Task<Dictionary<string, int>> GetPatientsByRhAsync()
        => await GetAsync<Dictionary<string, int>>($"{BaseUri}/rh-factor") ?? [];

    public async Task<Dictionary<string, int>> GetPatientGenderDistributionAsync()
        => await GetAsync<Dictionary<string, int>>($"{BaseUri}/gender-distribution") ?? [];

    public async Task<Dictionary<string, int>> GetConsultationDistributionAsync()
        => await GetAsync<Dictionary<string, int>>($"{BaseUri}/consultations") ?? [];

    public async Task<Dictionary<string, int>> GetTopDiagnosesAsync()
        => await GetAsync<Dictionary<string, int>>($"{BaseUri}/top-diagnoses") ?? [];

    public async Task<Dictionary<string, int>> GetMostPrescribedMedsAsync()
        => await GetAsync<Dictionary<string, int>>($"{BaseUri}/top-meds") ?? [];
}
