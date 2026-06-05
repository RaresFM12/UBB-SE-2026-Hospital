using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services;

public class PharmacyScheduleService(
    IShiftRepository shiftRepository,
    IStaffRepository staffRepository) : IPharmacyScheduleService
{
    public async Task<IReadOnlyList<Shift>> GetShiftsAsync(int pharmacistStaffId, DateTime rangeStart, DateTime rangeEnd, CancellationToken cancellationToken = default)
    {
        var shifts = await shiftRepository.GetByStaffIdAsync(pharmacistStaffId);
        bool IsInRange(Shift shift) => shift.StartTime < rangeEnd && shift.EndTime > rangeStart;
        return shifts.Where(IsInRange).OrderBy(shift => shift.StartTime).ToList();
    }

    public IReadOnlyList<Shift> GetShifts(int pharmacistStaffId, DateTime rangeStart, DateTime rangeEnd)
        => GetShiftsAsync(pharmacistStaffId, rangeStart, rangeEnd).GetAwaiter().GetResult();

    public async Task<IReadOnlyList<Pharmacyst>> GetPharmacistsAsync(CancellationToken cancellationToken = default)
        => await staffRepository.GetAllPharmacistsAsync();

    public IReadOnlyList<Pharmacyst> GetPharmacists()
        => GetPharmacistsAsync().GetAwaiter().GetResult();
}
