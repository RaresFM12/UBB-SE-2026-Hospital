namespace Hospital.Shared.Repositories
{
    using System.Collections.Generic;

    public interface IHangoutParticipantRepository
    {
        IReadOnlyList<(int HangoutId, int StaffId)> GetAllParticipants();

        void AddParticipant(int hangoutId, int staffId);
    }
}
