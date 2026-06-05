using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class PeriodTrackerApiClient(HttpClient httpClient) : ApiClientBase(httpClient), IPeriodTrackerService, IPeriodTrackerApiClient
{
    private const string BaseUri = "api/period-tracker";

    public async Task<User> GetUserAsync(int userId, CancellationToken cancellationToken = default)
        => await GetAsync<User>($"{BaseUri}/{userId}/user", cancellationToken) ?? new User();

    public PeriodTrackerState GetTrackerState(int userId)
        => Task.Run(async () => await GetAsync<PeriodTrackerState>($"{BaseUri}/{userId}/state") ?? new PeriodTrackerState()).GetAwaiter().GetResult();

    public PeriodTrackerDashboardSnapshot GetDashboardSnapshot(int userId, int monthOffset)
        => Task.Run(async () => await GetAsync<PeriodTrackerDashboardSnapshot>($"{BaseUri}/{userId}/dashboard?monthOffset={monthOffset}") ?? new PeriodTrackerDashboardSnapshot()).GetAwaiter().GetResult();

    public Dictionary<int, (string Body, bool IsDone)> GetNotes(int userId)
        => Task.Run(async () => await GetNotesAsync(userId)).GetAwaiter().GetResult();

    public async Task<Dictionary<int, (string Body, bool IsDone)>> GetNotesAsync(int userId, CancellationToken cancellationToken = default)
    {
        List<PeriodTrackerNoteDto> notes = await GetAsync<List<PeriodTrackerNoteDto>>($"{BaseUri}/{userId}/notes", cancellationToken) ?? [];
        return notes.ToDictionary(note => note.NoteId, note => (note.Body, note.IsDone));
    }

    public async Task UpdatePeriodTrackerAsync(int userId, DateTimeOffset startPeriodDate, double cycleDays, double periodLasts, int premenstrualSyndromeOption, CancellationToken cancellationToken = default)
        => await PutAsync($"{BaseUri}/{userId}", new { startPeriodDate, cycleDays, periodLasts, premenstrualSyndromeOption }, cancellationToken);

    public async Task AddNoteAsync(int userId, string noteBody, CancellationToken cancellationToken = default)
        => await PostAsync($"{BaseUri}/{userId}/notes", new { noteBody }, cancellationToken);

    public async Task UpdateNoteAsync(int userId, int noteId, string noteBody, bool isDone, CancellationToken cancellationToken = default)
        => await PutAsync($"{BaseUri}/{userId}/notes/{noteId}", new { noteBody, isDone }, cancellationToken);

    public async Task DeleteNoteAsync(int userId, int noteId, CancellationToken cancellationToken = default)
        => await DeleteAsync($"{BaseUri}/{userId}/notes/{noteId}", cancellationToken);

    public void UpdatePeriodTracker(int userId, DateTimeOffset startPeriodDate, double cycleDays, double periodLasts, int premenstrualSyndromeOption)
        => Task.Run(async () => await UpdatePeriodTrackerAsync(userId, startPeriodDate, cycleDays, periodLasts, premenstrualSyndromeOption)).GetAwaiter().GetResult();

    public void AddNote(int userId, string noteBody)
        => Task.Run(async () => await AddNoteAsync(userId, noteBody)).GetAwaiter().GetResult();

    public void UpdateNote(int userId, int noteId, string noteBody, bool isDone)
        => Task.Run(async () => await UpdateNoteAsync(userId, noteId, noteBody, isDone)).GetAwaiter().GetResult();

    public void DeleteNote(int userId, int noteId)
        => Task.Run(async () => await DeleteNoteAsync(userId, noteId)).GetAwaiter().GetResult();
}
