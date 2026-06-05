using Hospital.Data.Models;

namespace Hospital.Shared.Proxies;

public interface IPeriodTrackerApiClient
{
    Task<User> GetUserAsync(int userId, CancellationToken cancellationToken = default);
    PeriodTrackerState GetTrackerState(int userId);
    PeriodTrackerDashboardSnapshot GetDashboardSnapshot(int userId, int monthOffset);
    Dictionary<int, (string Body, bool IsDone)> GetNotes(int userId);
    Task<Dictionary<int, (string Body, bool IsDone)>> GetNotesAsync(int userId, CancellationToken cancellationToken = default);
    Task UpdatePeriodTrackerAsync(int userId, DateTimeOffset startPeriodDate, double cycleDays, double periodLasts, int premenstrualSyndromeOption, CancellationToken cancellationToken = default);
    Task AddNoteAsync(int userId, string noteBody, CancellationToken cancellationToken = default);
    Task UpdateNoteAsync(int userId, int noteId, string noteBody, bool isDone, CancellationToken cancellationToken = default);
    Task DeleteNoteAsync(int userId, int noteId, CancellationToken cancellationToken = default);

    void UpdatePeriodTracker(int userId, DateTimeOffset startPeriodDate, double cycleDays, double periodLasts, int premenstrualSyndromeOption);
    void AddNote(int userId, string noteBody);
    void UpdateNote(int userId, int noteId, string noteBody, bool isDone);
    void DeleteNote(int userId, int noteId);
}
