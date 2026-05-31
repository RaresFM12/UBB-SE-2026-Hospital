using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class HangoutService : IHangoutService
{
    public Task<IReadOnlyList<Hangout>> GetAllHangoutsAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<Hangout?> GetHangoutByIdAsync(int hangoutId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<int> CreateHangoutAsync(string title, string description, DateTime date, int maxParticipants, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<HangoutParticipant>> GetAllParticipantsAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task AddParticipantAsync(int hangoutId, int staffId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
