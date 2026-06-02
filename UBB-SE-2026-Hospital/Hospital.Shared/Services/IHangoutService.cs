using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IHangoutService
{
    Task<IReadOnlyList<Hangout>> GetAllHangoutsAsync(CancellationToken cancellationToken = default);
    List<Hangout> GetAllHangouts();
    Task<Hangout?> GetHangoutByIdAsync(int hangoutId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HangoutParticipant>> GetAllParticipantsAsync(CancellationToken cancellationToken = default);
    Task<int> CreateHangoutAsync(string title, string description, DateTime date, int maxParticipants, CancellationToken cancellationToken = default);
    int CreateHangout(string title, string description, DateTime date, int maxParticipants);
    int CreateHangout(string title, string description, DateTime date, int maxParticipants, Staff creator);
    Task AddParticipantAsync(int hangoutId, int staffId, CancellationToken cancellationToken = default);
    void JoinHangout(int hangoutId, int staffId);
    void JoinHangout(int hangoutId, Staff staff);
}
