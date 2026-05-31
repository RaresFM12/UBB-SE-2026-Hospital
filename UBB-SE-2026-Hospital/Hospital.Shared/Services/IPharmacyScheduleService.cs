using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IPharmacyScheduleService
{
    Task<IReadOnlyList<Shift>> GetShiftsAsync(int pharmacistStaffId, DateTime rangeStart, DateTime rangeEnd, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Pharmacyst>> GetPharmacistsAsync(CancellationToken cancellationToken = default);
}
