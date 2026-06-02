namespace Hospital.Shared.Repositories
{
    using System.Collections.Generic;
    using Hospital.Shared.Models;

    public interface IHangoutRepository
    {
        int AddHangout(string title, string description, System.DateTime date, int maximumParticipants);

        List<Hangout> GetAllHangouts();

        Hangout? GetHangoutById(int hangoutId);
    }
}
