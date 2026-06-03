using Hospital.Shared.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Proxy;

public class HttpERDispatchProxy(HttpClient httpClient) : ProxyBase(httpClient), IERDispatchService
{
    private const string BaseUri = "api/er-requests";

    public async Task<IReadOnlyList<int>> SimulateIncomingRequestsAsync(int count)
        => await PostAsync<object, List<int>>($"{BaseUri}/simulate", new { count }) ?? [];

    public async Task<IReadOnlyList<int>> GetPendingRequestIdsAsync()
        => await GetAsync<List<int>>($"{BaseUri}/pending") ?? [];

    public async Task<ERDispatchResult> DispatchERRequestAsync(int requestId)
        => await PostAsync<object, ERDispatchResult>($"{BaseUri}/{requestId}/dispatch", new { })
           ?? new ERDispatchResult { IsSuccess = false, Message = "Dispatch failed." };

    public async Task<ERDispatchResult> ManualOverrideAsync(int requestId, int doctorId, int nearEndMinutes)
        => await PostAsync<object, ERDispatchResult>($"{BaseUri}/{requestId}/override", new { doctorId, nearEndMinutes })
           ?? new ERDispatchResult { IsSuccess = false, Message = "Override failed." };

    public async Task<IReadOnlyList<DoctorProfile>> GetManualOverrideCandidatesAsync(int requestId, int nearEndMinutes)
        => await GetAsync<List<DoctorProfile>>($"{BaseUri}/{requestId}/candidates?nearEndMinutes={nearEndMinutes}") ?? [];

    public async Task<IReadOnlyList<ERRequest>> GetAllRequestsAsync()
        => await GetAsync<List<ERRequest>>(BaseUri) ?? [];

    public async Task<ERRequest?> GetRequestByIdAsync(int requestId)
        => await GetAsync<ERRequest>($"{BaseUri}/{requestId}");

    public async Task<ERRequest?> GetRequestByVisitIdAsync(int visitId)
        => await GetAsync<ERRequest>($"{BaseUri}/by-visit/{visitId}");

    public async Task<int> CreateRequestAsync(string specialization, string location, int? visitId = null)
        => await PostAsync<object, int>(BaseUri, new { specialization, location, visitId });

    public async Task UpdateRequestStatusAsync(int requestId, string status)
        => await PutAsync<object>($"{BaseUri}/{requestId}/status", new { status });

    public async Task<IReadOnlyList<ERDispatchResult>> DispatchAllPendingAsync()
        => await PostAsync<object, List<ERDispatchResult>>($"{BaseUri}/dispatch-all", new { }) ?? [];
}
