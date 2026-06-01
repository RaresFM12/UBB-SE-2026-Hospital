using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class ShiftManagementService(
    IStaffRepository staffRepository,
    IShiftRepository shiftRepository) : IShiftManagementService
{
    private const int DaysInWeek = 7;
    private const string PharmacyLocationLabel = "Pharmacy";

    public async Task<IReadOnlyList<Shift>> GetAllShiftsAsync(CancellationToken cancellationToken = default)
        => await shiftRepository.GetAllAsync();

    public async Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default)
        => await shiftRepository.GetByIdAsync(shiftId);

    public async Task CreateShiftAsync(int staffId, string location, DateTime startTime, DateTime endTime, ShiftStatus status, CancellationToken cancellationToken = default)
    {
        var staff = await staffRepository.GetByIdAsync(staffId)
            ?? throw new ArgumentException("Staff member not found.");
        if (!await ValidateNoOverlapAsync(staffId, startTime, endTime, cancellationToken))
        {
            throw new InvalidOperationException("Shift overlaps with an existing one.");
        }

        await shiftRepository.CreateAsync(new Shift(0, staff, location, startTime, endTime, status));
    }

    public async Task UpdateShiftStatusAsync(int shiftId, ShiftStatus status, CancellationToken cancellationToken = default)
    {
        var shift = await shiftRepository.GetByIdAsync(shiftId)
            ?? throw new ArgumentException("Shift not found.");
        shift.Status = status;
        await shiftRepository.UpdateAsync(shift);
    }

    public async Task UpdateShiftStaffAsync(int shiftId, int staffId, CancellationToken cancellationToken = default)
    {
        var shift = await shiftRepository.GetByIdAsync(shiftId)
            ?? throw new ArgumentException("Shift not found.");
        var staff = await staffRepository.GetByIdAsync(staffId)
            ?? throw new ArgumentException("Staff member not found.");
        if (!await ValidateNoOverlapAsync(staffId, shift.StartTime, shift.EndTime, cancellationToken))
        {
            throw new InvalidOperationException("Selected staff member is not free during the shift interval.");
        }

        shift.Staff = staff;
        await shiftRepository.UpdateAsync(shift);
    }

    public async Task DeleteShiftAsync(int shiftId, CancellationToken cancellationToken = default)
        => await shiftRepository.DeleteAsync(shiftId);

    public async Task<IReadOnlyList<Shift>> GetDailyShiftsAsync(DateTime date, CancellationToken cancellationToken = default)
        => (await shiftRepository.GetAllAsync())
            .Where(shift => shift.StartTime.Date == date.Date)
            .ToList();

    public async Task<IReadOnlyList<Shift>> GetWeeklyShiftsAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        int daysFromMonday = (DaysInWeek + (date.DayOfWeek - DayOfWeek.Monday)) % DaysInWeek;
        var weekStart = date.Date.AddDays(-daysFromMonday);
        var weekEnd = weekStart.AddDays(DaysInWeek);

        return (await shiftRepository.GetAllAsync())
            .Where(shift => shift.StartTime >= weekStart && shift.StartTime < weekEnd)
            .ToList();
    }

    public async Task<IReadOnlyList<Shift>> GetActiveShiftsAsync(CancellationToken cancellationToken = default)
        => (await shiftRepository.GetAllAsync())
            .Where(shift => shift.Status == ShiftStatus.Active)
            .ToList();

    public async Task<float> GetWeeklyHoursAsync(int staffId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        int daysFromMonday = (DaysInWeek + (now.DayOfWeek - DayOfWeek.Monday)) % DaysInWeek;
        var weekStart = now.Date.AddDays(-daysFromMonday);
        var weekEnd = weekStart.AddDays(DaysInWeek);

        return (await shiftRepository.GetByStaffIdAsync(staffId))
            .Where(shift => shift.StartTime >= weekStart && shift.StartTime < weekEnd)
            .Sum(shift => (float)(shift.EndTime - shift.StartTime).TotalHours);
    }

    public async Task<bool> ValidateNoOverlapAsync(int staffId, DateTime start, DateTime end, CancellationToken cancellationToken = default)
        => !(await shiftRepository.GetByStaffIdAsync(staffId))
            .Any(shift =>
                shift.Status != ShiftStatus.Completed &&
                shift.Status != ShiftStatus.Cancelled &&
                start < shift.EndTime &&
                end > shift.StartTime);

    public async Task<IReadOnlyList<Staff>> GetAllStaffAsync(CancellationToken cancellationToken = default)
        => await staffRepository.GetAllAsync();

    public async Task<Staff?> GetStaffByIdAsync(int staffId, CancellationToken cancellationToken = default)
        => await staffRepository.GetByIdAsync(staffId);

    public async Task<IReadOnlyList<Doctor>> GetDoctorsAsync(CancellationToken cancellationToken = default)
        => await staffRepository.GetAllDoctorsAsync();

    public async Task<IReadOnlyList<Pharmacyst>> GetPharmacistsAsync(CancellationToken cancellationToken = default)
        => await staffRepository.GetAllPharmacistsAsync();

    public async Task UpdateStaffStatusAsync(int staffId, string status, CancellationToken cancellationToken = default)
    {
        var staff = await staffRepository.GetByIdAsync(staffId)
            ?? throw new ArgumentException("Staff member not found.");
        staff.Status = status;
        await staffRepository.UpdateAsync(staff);
    }

    public async Task UpdateStaffAvailabilityAsync(int staffId, bool isAvailable, DoctorStatus status, CancellationToken cancellationToken = default)
    {
        var staff = await staffRepository.GetByIdAsync(staffId)
            ?? throw new ArgumentException("Staff member not found.");
        staff.Available = isAvailable;
        if (staff is Doctor doctor)
        {
            doctor.DoctorStatus = status;
            await staffRepository.UpdateAsync(doctor);
            return;
        }

        await staffRepository.UpdateAsync(staff);
    }

    public async Task<IReadOnlyList<Staff>> GetFilteredStaffAsync(string location, string requiredSpecializationOrCertification, CancellationToken cancellationToken = default)
    {
        var allStaff = await staffRepository.GetAllAsync();
        if (string.Equals(location, PharmacyLocationLabel, StringComparison.OrdinalIgnoreCase))
        {
            return allStaff
                .OfType<Pharmacyst>()
                .Where(staff => staff.Certification.Contains(requiredSpecializationOrCertification, StringComparison.OrdinalIgnoreCase))
                .Cast<Staff>()
                .ToList();
        }

        return allStaff
            .OfType<Doctor>()
            .Where(staff => staff.Specialization.Contains(requiredSpecializationOrCertification, StringComparison.OrdinalIgnoreCase))
            .Cast<Staff>()
            .ToList();
    }
}
