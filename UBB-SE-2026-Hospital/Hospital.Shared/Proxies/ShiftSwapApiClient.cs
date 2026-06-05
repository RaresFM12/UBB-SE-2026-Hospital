using System.Net.Http.Json;
using System.Text.Json;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class ShiftSwapApiClient : ApiClientBase, IShiftSwapService
{
    private const string BaseUri = "api/shift-swaps";

    public ShiftSwapApiClient(HttpClient httpClient) : base(httpClient)
    {
    }

    private sealed class ShiftSwapSummaryDto
    {
        public int SwapId { get; set; }
        public DateTime RequestedAt { get; set; }
        public JsonElement Status { get; set; }
        public int ShiftId { get; set; }
        public int RequesterId { get; set; }
        public int ColleagueId { get; set; }
    }

    private static ShiftSwapRequestStatus ParseStatus(JsonElement status)
    {
        try
        {
            if (status.ValueKind == JsonValueKind.Number && status.TryGetInt32(out int numeric)
                && Enum.IsDefined(typeof(ShiftSwapRequestStatus), numeric))
            {
                return (ShiftSwapRequestStatus)numeric;
            }

            if (status.ValueKind == JsonValueKind.String)
            {
                string? text = status.GetString();
                if (!string.IsNullOrWhiteSpace(text)
                    && Enum.TryParse<ShiftSwapRequestStatus>(text, ignoreCase: true, out var parsed))
                {
                    return parsed;
                }
            }
        }
        catch
        {
            // fall through to default
        }

        return ShiftSwapRequestStatus.PENDING;
    }

    private static ShiftSwapRequest MapToRequest(ShiftSwapSummaryDto summary) => new()
    {
        SwapId = summary.SwapId,
        RequestedAt = summary.RequestedAt,
        Status = ParseStatus(summary.Status),
        Shift = new Shift { Id = summary.ShiftId },
        Requester = new Staff { StaffId = summary.RequesterId },
        Colleague = new Staff { StaffId = summary.ColleagueId },
    };

    public async Task<IReadOnlyList<ShiftSwapRequest>> GetAllShiftSwapRequestsAsync(CancellationToken cancellationToken = default)
    {
        var summaries = await GetAsync<List<ShiftSwapSummaryDto>>(BaseUri, cancellationToken) ?? new List<ShiftSwapSummaryDto>();
        return summaries.Select(MapToRequest).ToList();
    }

    public async Task<ShiftSwapRequest?> GetShiftSwapByIdAsync(int swapId, CancellationToken cancellationToken = default)
    {
        try
        {
            var summary = await GetAsync<ShiftSwapSummaryDto>($"{BaseUri}/{swapId}", cancellationToken);
            return summary is null ? null : MapToRequest(summary);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    public async Task<int> CreateShiftSwapRequestAsync(int shiftId, int requesterId, int colleagueId, DateTime requestedAt, ShiftSwapRequestStatus status, CancellationToken cancellationToken = default)
    {
        return await PostAsync<object, int>(
            BaseUri,
            new
            {
                ShiftId = shiftId,
                RequesterId = requesterId,
                ColleagueId = colleagueId,
                RequestedAt = requestedAt,
                Status = status.ToString(),
            },
            cancellationToken);
    }

    public async Task UpdateShiftSwapStatusAsync(int swapId, string status, CancellationToken cancellationToken = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Patch, $"{BaseUri}/{swapId}/status")
        {
            Content = JsonContent.Create(new { Status = status }),
        };
        var resp = await HttpClient.SendAsync(req, cancellationToken);
        resp.EnsureSuccessStatusCode();
    }

    public async Task<bool> AcceptSwapRequestAsync(int swapId, int colleagueId, CancellationToken cancellationToken = default)
    {
        await UpdateShiftSwapStatusAsync(swapId, "ACCEPTED", cancellationToken);
        return true;
    }

    public async Task<bool> RejectSwapRequestAsync(int swapId, int colleagueId, CancellationToken cancellationToken = default)
    {
        await UpdateShiftSwapStatusAsync(swapId, "REJECTED", cancellationToken);
        return true;
    }

    // ----- Not available in the desktop client -----

    public List<ShiftSwapRequest> GetAllShiftSwapRequests()
        => throw new NotSupportedException("Not available in the desktop client.");

    public void RequestShiftSwap(int requesterId, int shiftId, int colleagueId, out string message)
        => throw new NotSupportedException("Not available in the desktop client.");

    public Task<IReadOnlyList<Shift>> GetFutureShiftsForStaffAsync(int staffId, CancellationToken cancellationToken = default)
        => Task.FromException<IReadOnlyList<Shift>>(new NotSupportedException("Not available in the desktop client."));

    public List<Shift> GetFutureShiftsForStaff(int staffId)
        => throw new NotSupportedException("Not available in the desktop client.");

    public Task<IReadOnlyList<Staff>> GetEligibleSwapColleaguesAsync(int requesterId, int shiftId, CancellationToken cancellationToken = default)
        => Task.FromException<IReadOnlyList<Staff>>(new NotSupportedException("Not available in the desktop client."));

    public List<IStaff> GetEligibleSwapColleaguesForShift(int requesterId, int shiftId, out string error)
        => throw new NotSupportedException("Not available in the desktop client.");

    public void AcceptSwapRequest(int swapId, int colleagueId, out string message)
        => throw new NotSupportedException("Not available in the desktop client.");

    public void RejectSwapRequest(int swapId, int colleagueId, out string message)
        => throw new NotSupportedException("Not available in the desktop client.");

    public List<Doctor> GetAllDoctors()
        => throw new NotSupportedException("Not available in the desktop client.");

    public List<ShiftSwapRequest> GetIncomingSwapRequests(int staffId)
        => throw new NotSupportedException("Not available in the desktop client.");
}
