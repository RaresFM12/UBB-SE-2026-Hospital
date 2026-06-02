using Hospital.Data.Models;
using Hospital.Shared.Models.StaffPharmacy;

namespace Hospital.Shared.Services;

public interface IPeriodTrackerService
{
    Task<User> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    PeriodTrackerState GetTrackerState();
    PeriodTrackerDashboardSnapshot GetDashboardSnapshot(int monthOffset);
    Task<Dictionary<int, (string Body, bool IsDone)>> GetNotesAsync(CancellationToken cancellationToken = default);
    Dictionary<int, (string Body, bool IsDone)> GetNotes();
    Task UpdatePeriodTrackerAsync(DateTimeOffset startPeriodDate, double cycleDays, double periodLasts, int premenstrualSyndromeOption, CancellationToken cancellationToken = default);
    void UpdatePeriodTracker(DateTimeOffset startPeriodDate, double cycleDays, double periodLasts, int premenstrualSyndromeOption);
    Task AddNoteAsync(string noteBody, CancellationToken cancellationToken = default);
    void AddNote(string noteBody);
    Task UpdateNoteAsync(int noteId, string noteBody, bool isDone, CancellationToken cancellationToken = default);
    void UpdateNote(int noteId, string noteBody, bool isDone);
    Task DeleteNoteAsync(int noteId, CancellationToken cancellationToken = default);
    void DeleteNote(int noteId);
    void SaveCurrentUser();
}
