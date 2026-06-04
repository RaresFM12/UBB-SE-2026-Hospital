namespace Hospital.Shared.Repositories
{
    using System.Collections.Generic;
    using Hospital.Shared.Models;

    public interface IPharmacyShiftRepository
    {
        IReadOnlyList<Shift> GetAllShifts();

        void AddShift(Shift shift);
    }
}
