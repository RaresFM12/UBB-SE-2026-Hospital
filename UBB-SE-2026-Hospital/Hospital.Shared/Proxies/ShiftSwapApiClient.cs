using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class ShiftSwapApiClient(HttpClient httpClient) : ApiClientBase(httpClient), IShiftSwapService, IShiftSwapApiClient
{
    private const string BaseUri = "api/shift-swaps";
    private const string ShiftsUri = "api/shifts";
    private const string DoctorsUri = "api/staff/doctors";

    private static ShiftSwapRequest ToRequest(ShiftSwapSummaryDto summary) => new()
    {
        SwapId = summary.SwapId,
        RequestedAt = summary.RequestedAt,
        Status = summary.Status,
        Shift = new Shift { Id = summary.ShiftId },
        Requester = new Staff { StaffId = summary.RequesterId },
        Colleague = new Staff { StaffId = summary.ColleagueId },
    };

    public async Task<IReadOnlyList<ShiftSwapRequest>> GetAllShiftSwapRequestsAsync(CancellationToken cancellationToken = default)
    {
        List<ShiftSwapSummaryDto> summaries = await GetAsync<List<ShiftSwapSummaryDto>>(BaseUri, cancellationToken) ?? [];
        return summaries.Select(ToRequest).ToList();
    }

    public List<ShiftSwapRequest> GetAllShiftSwapRequests()
        => Task.Run(async () => (await GetAllShiftSwapRequestsAsync()).ToList()).GetAwaiter().GetResult();

    public async Task<ShiftSwapRequest?> GetShiftSwapByIdAsync(int swapId, CancellationToken cancellationToken = default)
    {
        ShiftSwapSummaryDto? summary = await GetAsync<ShiftSwapSummaryDto>($"{BaseUri}/{swapId}", cancellationToken);
        return summary is null ? null : ToRequest(summary);
    }

    public async Task<int> CreateShiftSwapRequestAsync(int shiftId, int requesterId, int colleagueId, DateTime requestedAt, ShiftSwapRequestStatus status, CancellationToken cancellationToken = default)
        => await PostAsync<object, int>(BaseUri, new { shiftId, requesterId, colleagueId, requestedAt, status }, cancellationToken);

    public void RequestShiftSwap(int requesterId, int shiftId, int colleagueId, out string message)
    {
        try
        {
            Task.Run(async () => await CreateShiftSwapRequestAsync(shiftId, requesterId, colleagueId, DateTime.UtcNow, ShiftSwapRequestStatus.PENDING)).GetAwaiter().GetResult();
            message = "Shift swap request created.";
        }
        catch (Exception exception)
        {
            message = exception.Message;
        }
    }

    public async Task UpdateShiftSwapStatusAsync(int swapId, string status, CancellationToken cancellationToken = default)
        => await PatchStatusAsync(swapId, status, cancellationToken);

    private async Task PatchStatusAsync(int swapId, string status, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"{BaseUri}/{swapId}/status")
        {
            Content = System.Net.Http.Json.JsonContent.Create(new { status }, options: JsonOptions),
        };
        using HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<Shift>> GetFutureShiftsForStaffAsync(int staffId, CancellationToken cancellationToken = default)
    {
        List<Shift> shifts = await GetAsync<List<Shift>>(ShiftsUri, cancellationToken) ?? [];
        return shifts.Where(shift => shift.Staff?.StaffId == staffId && shift.StartTime > DateTime.Now).ToList();
    }

    public List<Shift> GetFutureShiftsForStaff(int staffId)
        => Task.Run(async () => (await GetFutureShiftsForStaffAsync(staffId)).ToList()).GetAwaiter().GetResult();

    // No dedicated endpoint for eligible-colleague resolution; returns empty until exposed by the API.
    public Task<IReadOnlyList<Staff>> GetEligibleSwapColleaguesAsync(int requesterId, int shiftId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Staff>>([]);

    public List<IStaff> GetEligibleSwapColleaguesForShift(int requesterId, int shiftId, out string error)
    {
        error = string.Empty;
        return [];
    }

    public async Task<bool> AcceptSwapRequestAsync(int swapId, int colleagueId, CancellationToken cancellationToken = default)
    {
        await UpdateShiftSwapStatusAsync(swapId, ShiftSwapRequestStatus.ACCEPTED.ToString(), cancellationToken);
        return true;
    }

    public void AcceptSwapRequest(int swapId, int colleagueId, out string message)
    {
        try
        {
            Task.Run(async () => await AcceptSwapRequestAsync(swapId, colleagueId)).GetAwaiter().GetResult();
            message = "Shift swap accepted.";
        }
        catch (Exception exception)
        {
            message = exception.Message;
        }
    }

    public async Task<bool> RejectSwapRequestAsync(int swapId, int colleagueId, CancellationToken cancellationToken = default)
    {
        await UpdateShiftSwapStatusAsync(swapId, ShiftSwapRequestStatus.REJECTED.ToString(), cancellationToken);
        return true;
    }

    public void RejectSwapRequest(int swapId, int colleagueId, out string message)
    {
        try
        {
            Task.Run(async () => await RejectSwapRequestAsync(swapId, colleagueId)).GetAwaiter().GetResult();
            message = "Shift swap rejected.";
        }
        catch (Exception exception)
        {
            message = exception.Message;
        }
    }

    public List<Doctor> GetAllDoctors()
        => Task.Run(async () =>
        {
            List<DoctorSummaryDto> doctorSummaries = await GetAsync<List<DoctorSummaryDto>>(DoctorsUri) ?? [];
            return doctorSummaries.Select(doctorSummary => new Doctor { StaffId = doctorSummary.StaffId, FirstName = doctorSummary.FirstName, LastName = doctorSummary.LastName }).ToList();
        }).GetAwaiter().GetResult();

    public List<ShiftSwapRequest> GetIncomingSwapRequests(int staffId)
        => GetAllShiftSwapRequests().Where(swapRequest => swapRequest.Colleague?.StaffId == staffId).ToList();

    private sealed class ShiftSwapSummaryDto
    {
        public int SwapId { get; set; }
        public DateTime RequestedAt { get; set; }
        public ShiftSwapRequestStatus Status { get; set; }
        public int ShiftId { get; set; }
        public int RequesterId { get; set; }
        public int ColleagueId { get; set; }
    }

    private sealed class DoctorSummaryDto
    {
        public int StaffId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
