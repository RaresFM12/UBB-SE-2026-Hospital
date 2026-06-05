using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Proxy;

public class HttpERVisitProxy(HttpClient httpClient) : ProxyBase(httpClient), IERVisitService
{
    private const string BaseUri = "api/ervisits";

    public async Task<List<ERVisit>> GetAllAsync()
        => await GetAsync<List<ERVisit>>(BaseUri) ?? [];

    public async Task<ERVisit?> GetByIdAsync(int id)
        => await GetAsync<ERVisit>($"{BaseUri}/{id}");

    public async Task<List<ERVisit>> GetByPatientIdAsync(int patientId)
        => await GetAsync<List<ERVisit>>($"{BaseUri}/patient/{patientId}") ?? [];

    public async Task<List<ERVisit>> GetActiveVisitsAsync()
        => await GetAsync<List<ERVisit>>($"{BaseUri}/active") ?? [];

    public async Task<List<ERVisit>> GetByStatusAsync(string status)
        => await GetAsync<List<ERVisit>>($"{BaseUri}/status/{status}") ?? [];

    public async Task<ERVisit> CreateAsync(ERVisit visit)
        => await PostAsync<ERVisit, ERVisit>(BaseUri, visit) ?? visit;

    public async Task<ERVisit> UpdateAsync(ERVisit visit)
    {
        await PutAsync($"{BaseUri}/{visit.VisitId}", visit);
        return visit;
    }

    public async Task DeleteAsync(int id)
        => await DeleteAsync($"{BaseUri}/{id}");

    public async Task<bool> AutoAssignHighestPriorityRoomAsync()
    {
        bool? assigned = await PostAsync<object, bool>($"{BaseUri}/auto-assign-room", new { });
        return assigned ?? false;
    }

    public async Task AssignRoomAsync(int visitId, int roomId)
        => await PostAsync<object, object>($"{BaseUri}/{visitId}/assign-room/{roomId}", new { });

    public async Task TransferVisitAsync(int visitId)
        => await PostAsync<object, object>($"{BaseUri}/{visitId}/transfer", new { });

    public async Task RetryTransferAsync(int visitId)
        => await PostAsync<object, object>($"{BaseUri}/{visitId}/retry-transfer", new { });

    public async Task CloseVisitAsync(int visitId)
        => await PostAsync<object, object>($"{BaseUri}/{visitId}/close", new { });

}
