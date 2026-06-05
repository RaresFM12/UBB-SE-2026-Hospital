using Hospital.Data.Models;
using Hospital.Shared.Models.StaffPharmacy;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Proxy;

public class HttpERDispatchProxy(HttpClient httpClient) : ProxyBase(httpClient), IERDispatchService
{
    private const string BaseUri = "api/er-requests";

    public async Task<IReadOnlyList<ERRequest>> GetAllRequestsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<ERRequest>>(BaseUri) ?? [];

    public async Task<ERRequest?> GetRequestByIdAsync(int requestId, CancellationToken cancellationToken = default)
        => await GetAsync<ERRequest>($"{BaseUri}/{requestId}");

    public async Task<int> CreateRequestAsync(string specialization, string location, string status, CancellationToken cancellationToken = default)
        => await PostAsync<object, int>(BaseUri, new { specialization, location, status });

    public async Task<int> CreateRequestAsync(string specialization, string location, CancellationToken cancellationToken = default)
        => await PostAsync<object, int>(BaseUri, new { specialization, location });

    public async Task UpdateRequestStatusAsync(int requestId, string status, int? assignedDoctorId, string? assignedDoctorName, CancellationToken cancellationToken = default)
        => await PutAsync<object>($"{BaseUri}/{requestId}/status", new { status, assignedDoctorId, assignedDoctorName });

    public async Task UpdateRequestStatusAsync(int requestId, string status, CancellationToken cancellationToken = default)
        => await PutAsync<object>($"{BaseUri}/{requestId}/status", new { status });

    public async Task<IReadOnlyList<int>> GetPendingRequestIdsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<int>>($"{BaseUri}/pending") ?? [];

    public async Task<ERDispatchResult> DispatchERRequestAsync(int requestId, CancellationToken cancellationToken = default)
        => await PostAsync<object, ERDispatchResult>($"{BaseUri}/{requestId}/dispatch", new { })
           ?? new ERDispatchResult { IsSuccess = false, Message = "Dispatch failed." };

    public async Task<ERDispatchResult> ManualOverrideAsync(int requestId, int doctorId, int nearEndMinutes, CancellationToken cancellationToken = default)
        => await PostAsync<object, ERDispatchResult>($"{BaseUri}/{requestId}/override", new { doctorId, nearEndMinutes })
           ?? new ERDispatchResult { IsSuccess = false, Message = "Override failed." };

    public async Task<IReadOnlyList<ERDispatchResult>> DispatchAllPendingAsync(CancellationToken cancellationToken = default)
        => await PostAsync<object, List<ERDispatchResult>>($"{BaseUri}/dispatch-all", new { }) ?? [];

    public async Task<IReadOnlyList<int>> SimulateIncomingRequestsAsync(int count, CancellationToken cancellationToken = default)
        => await PostAsync<object, List<int>>($"{BaseUri}/simulate", new { count }) ?? [];

    public async Task<IReadOnlyList<DoctorProfile>> GetManualOverrideCandidatesAsync(int requestId, int nearEndMinutes, CancellationToken cancellationToken = default)
        => await GetAsync<List<DoctorProfile>>($"{BaseUri}/{requestId}/candidates?nearEndMinutes={nearEndMinutes}") ?? [];

    public async Task<ERRequest?> GetRequestByVisitIdAsync(int visitId)
        => await GetAsync<ERRequest>($"{BaseUri}/by-visit/{visitId}");
}
