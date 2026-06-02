namespace Hospital.Shared.Repositories
{
    using System.Collections.Generic;
    using Hospital.Shared.Models;

    public interface IShiftManagementShiftRepository
    {
        IReadOnlyList<Shift> GetAllShifts();

        void AddShift(Shift newShift);

        void UpdateShiftStatus(int shiftId, ShiftStatus status);

        void UpdateShiftStaffId(int shiftId, int newStaffId);
    }
}
