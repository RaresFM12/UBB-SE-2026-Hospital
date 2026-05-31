using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IHangoutService
{
    Task<IReadOnlyList<Hangout>> GetAllHangoutsAsync(CancellationToken cancellationToken = default);

    Task<Hangout?> GetHangoutByIdAsync(int hangoutId, CancellationToken cancellationToken = default);

    Task<int> CreateHangoutAsync(string title, string description, DateTime date, int maxParticipants, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HangoutParticipant>> GetAllParticipantsAsync(CancellationToken cancellationToken = default);

    Task AddParticipantAsync(int hangoutId, int staffId, CancellationToken cancellationToken = default);
}
