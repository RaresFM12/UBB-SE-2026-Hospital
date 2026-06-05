using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IShiftManagementService
{
    Task<IReadOnlyList<Shift>> GetAllShiftsAsync(CancellationToken cancellationToken = default);
    Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default);
    Task CreateShiftAsync(int staffId, string location, DateTime startTime, DateTime endTime, ShiftStatus status, CancellationToken cancellationToken = default);
    bool TryAddShift(IStaff staff, DateTime startTime, DateTime endTime, string location);
    Task UpdateShiftStatusAsync(int shiftId, ShiftStatus status, CancellationToken cancellationToken = default);
    void CancelShift(int shiftId);
    void SetShiftActive(int shiftId);
    Task UpdateShiftStaffAsync(int shiftId, int staffId, CancellationToken cancellationToken = default);
    Task DeleteShiftAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Shift>> GetDailyShiftsAsync(DateTime date, CancellationToken cancellationToken = default);
    IReadOnlyList<Shift> GetDailyShifts(DateTime date);
    Task<IReadOnlyList<Shift>> GetWeeklyShiftsAsync(DateTime date, CancellationToken cancellationToken = default);
    IReadOnlyList<Shift> GetWeeklyShifts(DateTime date);
    Task<IReadOnlyList<Shift>> GetActiveShiftsAsync(CancellationToken cancellationToken = default);
    Task<float> GetWeeklyHoursAsync(int staffId, CancellationToken cancellationToken = default);
    Task<bool> ValidateNoOverlapAsync(int staffId, DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Staff>> GetAllStaffAsync(CancellationToken cancellationToken = default);
    Task<Staff?> GetStaffByIdAsync(int staffId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Doctor>> GetDoctorsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Pharmacyst>> GetPharmacistsAsync(CancellationToken cancellationToken = default);
    Task UpdateStaffStatusAsync(int staffId, string status, CancellationToken cancellationToken = default);
    Task UpdateStaffAvailabilityAsync(int staffId, bool isAvailable, DoctorStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Staff>> GetFilteredStaffAsync(string location, string requiredSpecializationOrCertification, CancellationToken cancellationToken = default);
    bool ValidateShiftTimes(TimeSpan startTime, TimeSpan endTime);
    IReadOnlyList<string> GetSpecializationsAndCertificationsForLocation(string location);
    IReadOnlyList<IStaff> GetFilteredStaff(string location, string requiredSpecializationOrCertification);
    IReadOnlyList<IStaff> FindStaffReplacements(Shift shift);
    bool ReassignShift(Shift shift, IStaff replacement);
}
