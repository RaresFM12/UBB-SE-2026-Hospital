namespace Hospital.Shared.Repositories
{
    using System.Collections.Generic;
    using Hospital.Shared.Models;

    public interface IShiftManagementStaffRepository
    {
        List<IStaff> LoadAllStaff();

        void UpdateStaffAvailability(int staffId, bool isAvailable, DoctorStatus status = DoctorStatus.OFF_DUTY);
    }
}
