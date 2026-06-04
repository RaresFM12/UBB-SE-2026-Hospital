namespace Hospital.Shared.Services
{
    using System;
    using System.Collections.Generic;
    using Hospital.Shared.Models;

    public interface IHangoutService
    {
        int CreateHangout(string title, string description, DateTime date, int maxParticipants, IStaff creator);

        void JoinHangout(int hangoutId, IStaff staff);

        List<Hangout> GetAllHangouts();
    }
}
