using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class ShiftManagementService : IShiftManagementService
{
    public Task<IReadOnlyList<Shift>> GetAllShiftsAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task CreateShiftAsync(int staffId, string location, DateTime startTime, DateTime endTime, ShiftStatus status, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task UpdateShiftStatusAsync(int shiftId, ShiftStatus status, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task UpdateShiftStaffAsync(int shiftId, int staffId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task DeleteShiftAsync(int shiftId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<Shift>> GetDailyShiftsAsync(DateTime date, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<Shift>> GetWeeklyShiftsAsync(DateTime date, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<Shift>> GetActiveShiftsAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<float> GetWeeklyHoursAsync(int staffId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> ValidateNoOverlapAsync(int staffId, DateTime start, DateTime end, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<Staff>> GetAllStaffAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<Staff?> GetStaffByIdAsync(int staffId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<Doctor>> GetDoctorsAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<Pharmacyst>> GetPharmacistsAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task UpdateStaffStatusAsync(int staffId, string status, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task UpdateStaffAvailabilityAsync(int staffId, bool isAvailable, DoctorStatus status, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<Staff>> GetFilteredStaffAsync(string location, string requiredSpecializationOrCertification, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
