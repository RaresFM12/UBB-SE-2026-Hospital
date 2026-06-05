using Hospital.Data.Models;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Proxy;

public class HttpTriageProxy(HttpClient httpClient) : ProxyBase(httpClient), ITriageService
{
    private const string BaseUri = "api/triage";

    public async Task<List<Triage>> GetAllAsync()
        => await GetAsync<List<Triage>>(BaseUri) ?? [];

    public async Task<Triage?> GetByIdAsync(int id)
        => await GetAsync<Triage>($"{BaseUri}/{id}");

    public async Task<Triage> CreateAsync(Triage triage)
        => await PostAsync<Triage, Triage>(BaseUri, triage) ?? triage;

    public async Task<Triage> UpdateAsync(Triage triage)
    {
        await PutAsync($"{BaseUri}/{triage.TriageId}", triage);
        return triage;
    }

    public async Task DeleteAsync(int id)
        => await DeleteAsync($"{BaseUri}/{id}");

    public async Task<Triage?> GetByVisitIdAsync(int visitId)
        => await GetAsync<Triage>($"{BaseUri}/visit/{visitId}");

    public async Task<Triage> CreateTriageAsync(int visitId, PerformTriageRequest parameters)
        => await PostAsync<PerformTriageRequest, Triage>($"{BaseUri}/visit/{visitId}", parameters)
           ?? throw new InvalidOperationException("Failed to create triage.");

    public async Task MoveVisitToQueueAsync(int visitId)
        => await PostAsync<object, object>($"api/ervisits/{visitId}/move-to-queue", new { });

    public async Task CloseVisitAsync(int visitId)
        => await PostAsync<object, object>($"api/ervisits/{visitId}/close", new { });

    public async Task<List<ERVisit>> GetVisitsForTriageAsync()
        => await GetAsync<List<ERVisit>>("api/ervisits/for-triage") ?? [];
}
