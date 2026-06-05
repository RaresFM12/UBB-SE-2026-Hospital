using Hospital.Data.Models;

namespace Hospital.Shared.Proxies
{
    public interface IShiftManagementApiClient
    {
        void CancelShift(int shiftId);
        Task CreateShiftAsync(int staffId, string location, DateTime startTime, DateTime endTime, ShiftStatus status, CancellationToken cancellationToken = default);
        Task DeleteShiftAsync(int shiftId, CancellationToken cancellationToken = default);
        IReadOnlyList<IStaff> FindStaffReplacements(Shift shift);
        Task<IReadOnlyList<Shift>> GetActiveShiftsAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Shift>> GetAllShiftsAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Staff>> GetAllStaffAsync(CancellationToken cancellationToken = default);
        IReadOnlyList<Shift> GetDailyShifts(DateTime date);
        Task<IReadOnlyList<Shift>> GetDailyShiftsAsync(DateTime date, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Doctor>> GetDoctorsAsync(CancellationToken cancellationToken = default);
        IReadOnlyList<IStaff> GetFilteredStaff(string location, string requiredSpecializationOrCertification);
        Task<IReadOnlyList<Staff>> GetFilteredStaffAsync(string location, string requiredSpecializationOrCertification, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Pharmacyst>> GetPharmacistsAsync(CancellationToken cancellationToken = default);
        Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default);
        IReadOnlyList<string> GetSpecializationsAndCertificationsForLocation(string location);
        Task<Staff?> GetStaffByIdAsync(int staffId, CancellationToken cancellationToken = default);
        Task<float> GetWeeklyHoursAsync(int staffId, CancellationToken cancellationToken = default);
        IReadOnlyList<Shift> GetWeeklyShifts(DateTime date);
        Task<IReadOnlyList<Shift>> GetWeeklyShiftsAsync(DateTime date, CancellationToken cancellationToken = default);
        bool IsStaffWorkingDuring(int staffId, DateTime startTime, DateTime endTime);
        bool ReassignShift(Shift shift, IStaff newStaff);
        void SetShiftActive(int shiftId);
        bool TryAddShift(IStaff staff, DateTime start, DateTime end, string location);
        Task UpdateShiftStaffAsync(int shiftId, int staffId, CancellationToken cancellationToken = default);
        Task UpdateShiftStatusAsync(int shiftId, ShiftStatus status, CancellationToken cancellationToken = default);
        Task UpdateStaffAvailabilityAsync(int staffId, bool isAvailable, DoctorStatus status, CancellationToken cancellationToken = default);
        Task UpdateStaffStatusAsync(int staffId, string status, CancellationToken cancellationToken = default);
        Task<bool> ValidateNoOverlapAsync(int staffId, DateTime start, DateTime end, CancellationToken cancellationToken = default);
        bool ValidateShiftTimes(TimeSpan start, TimeSpan end);
    }
}