using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IShiftManagementService
{
    // Shifts
    Task<IReadOnlyList<Shift>> GetAllShiftsAsync(CancellationToken cancellationToken = default);

    Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default);

    Task CreateShiftAsync(int staffId, string location, DateTime startTime, DateTime endTime, ShiftStatus status, CancellationToken cancellationToken = default);

    Task UpdateShiftStatusAsync(int shiftId, ShiftStatus status, CancellationToken cancellationToken = default);

    Task UpdateShiftStaffAsync(int shiftId, int staffId, CancellationToken cancellationToken = default);

    Task DeleteShiftAsync(int shiftId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Shift>> GetDailyShiftsAsync(DateTime date, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Shift>> GetWeeklyShiftsAsync(DateTime date, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Shift>> GetActiveShiftsAsync(CancellationToken cancellationToken = default);

    Task<float> GetWeeklyHoursAsync(int staffId, CancellationToken cancellationToken = default);

    Task<bool> ValidateNoOverlapAsync(int staffId, DateTime start, DateTime end, CancellationToken cancellationToken = default);

    // Staff
    Task<IReadOnlyList<Staff>> GetAllStaffAsync(CancellationToken cancellationToken = default);

    Task<Staff?> GetStaffByIdAsync(int staffId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Doctor>> GetDoctorsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Pharmacyst>> GetPharmacistsAsync(CancellationToken cancellationToken = default);

    Task UpdateStaffStatusAsync(int staffId, string status, CancellationToken cancellationToken = default);

    Task UpdateStaffAvailabilityAsync(int staffId, bool isAvailable, DoctorStatus status, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Staff>> GetFilteredStaffAsync(string location, string requiredSpecializationOrCertification, CancellationToken cancellationToken = default);
}
