namespace Hospital.Shared.Services
{
    using System;
    using System.Collections.Generic;
    using Hospital.Shared.Models;

    public interface IPharmacyVacationService
    {
        IReadOnlyList<Pharmacyst> GetPharmacists();

        void RegisterVacation(int pharmacistStaffId, DateTime startDate, DateTime endDate);
    }
}
