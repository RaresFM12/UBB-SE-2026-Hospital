using Hospital.Data.Models;
using Hospital.Shared.Models.StaffPharmacy;

namespace Hospital.Shared.Services;

public interface IPeriodTrackerService
{
    Task<User> GetCurrentUserAsync(CancellationToken cancellationToken = default);

    PeriodTrackerState GetTrackerState();

    PeriodTrackerDashboardSnapshot GetDashboardSnapshot(int monthOffset);

    Task<Dictionary<int, (string Body, bool IsDone)>> GetNotesAsync(CancellationToken cancellationToken = default);

    Task UpdatePeriodTrackerAsync(DateTimeOffset startPeriodDate, double cycleDays, double periodLasts, int premenstrualSyndromeOption, CancellationToken cancellationToken = default);

    Task AddNoteAsync(string noteBody, CancellationToken cancellationToken = default);

    Task UpdateNoteAsync(int noteId, string noteBody, bool isDone, CancellationToken cancellationToken = default);

    Task DeleteNoteAsync(int noteId, CancellationToken cancellationToken = default);
}
