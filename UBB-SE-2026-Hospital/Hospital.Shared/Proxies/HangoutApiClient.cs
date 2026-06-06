using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class HangoutApiClient(HttpClient httpClient) : ApiClientBase(httpClient), IHangoutService, IHangoutApiClient
{
    private const string BaseUri = "api/hangouts";
    private const string ParticipantsUri = "api/hangout-participants";

    public async Task<IReadOnlyList<Hangout>> GetAllHangoutsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<Hangout>>(BaseUri, cancellationToken) ?? [];

    public List<Hangout> GetAllHangouts()
        => Task.Run(async () => await GetAsync<List<Hangout>>(BaseUri) ?? []).GetAwaiter().GetResult();

    public async Task<Hangout?> GetHangoutByIdAsync(int hangoutId, CancellationToken cancellationToken = default)
        => await GetAsync<Hangout>($"{BaseUri}/{hangoutId}", cancellationToken);

    public async Task<IReadOnlyList<HangoutParticipant>> GetAllParticipantsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<HangoutParticipant>>(ParticipantsUri, cancellationToken) ?? [];

    public async Task<int> CreateHangoutAsync(string title, string description, DateTime date, int maxParticipants, CancellationToken cancellationToken = default)
        => await PostAsync<object, int>(BaseUri, new { title, description, date, maxParticipants }, cancellationToken);

    public int CreateHangout(string title, string description, DateTime date, int maxParticipants)
        => Task.Run(async () => await CreateHangoutAsync(title, description, date, maxParticipants)).GetAwaiter().GetResult();

    public int CreateHangout(string title, string description, DateTime date, int maxParticipants, Staff creator)
        => Task.Run(async () => await CreateHangoutAsync(title, description, date, maxParticipants)).GetAwaiter().GetResult();

    public async Task AddParticipantAsync(int hangoutId, int staffId, CancellationToken cancellationToken = default)
        => await PostAsync($"{ParticipantsUri}", new { hangoutId, staffId }, cancellationToken);

    public void JoinHangout(int hangoutId, int staffId)
        => Task.Run(async () => await AddParticipantAsync(hangoutId, staffId)).GetAwaiter().GetResult();

    public void JoinHangout(int hangoutId, Staff staff)
        => Task.Run(async () => await AddParticipantAsync(hangoutId, staff.StaffId)).GetAwaiter().GetResult();
}
