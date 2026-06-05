using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class HangoutApiClient : ApiClientBase, IHangoutService
{
    private const string BaseUri = "api/hangouts";
    private const string ParticipantsUri = "api/hangout-participants";

    public HangoutApiClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<Hangout>> GetAllHangoutsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<Hangout>>(BaseUri, cancellationToken) ?? new List<Hangout>();

    public Task<Hangout?> GetHangoutByIdAsync(int hangoutId, CancellationToken cancellationToken = default)
        => GetAsync<Hangout>($"{BaseUri}/{hangoutId}", cancellationToken);

    public async Task<IReadOnlyList<HangoutParticipant>> GetAllParticipantsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<HangoutParticipant>>(ParticipantsUri, cancellationToken) ?? new List<HangoutParticipant>();

    public async Task<int> CreateHangoutAsync(string title, string description, DateTime date, int maxParticipants, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            Title = title,
            Description = description,
            Date = date,
            MaxParticipants = maxParticipants,
        };

        return await PostAsync<object, int>(BaseUri, payload, cancellationToken);
    }

    public Task AddParticipantAsync(int hangoutId, int staffId, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            HangoutId = hangoutId,
            StaffId = staffId,
        };

        return PostAsync(ParticipantsUri, payload, cancellationToken);
    }

    // Synchronous interface members are not supported in the desktop client.
    public List<Hangout> GetAllHangouts()
        => throw new NotSupportedException("Not available in the desktop client.");

    public int CreateHangout(string title, string description, DateTime date, int maxParticipants)
        => throw new NotSupportedException("Not available in the desktop client.");

    public int CreateHangout(string title, string description, DateTime date, int maxParticipants, Staff creator)
        => throw new NotSupportedException("Not available in the desktop client.");

    public void JoinHangout(int hangoutId, int staffId)
        => throw new NotSupportedException("Not available in the desktop client.");

    public void JoinHangout(int hangoutId, Staff staff)
        => throw new NotSupportedException("Not available in the desktop client.");
}
