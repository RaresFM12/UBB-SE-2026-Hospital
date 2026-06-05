using Hospital.Data.Models;

namespace Hospital.Shared.Proxies;

public interface IPharmacyScheduleApiClient
{
    Task<IReadOnlyList<Shift>> GetShiftsAsync(int pharmacistStaffId, DateTime rangeStart, DateTime rangeEnd, CancellationToken cancellationToken = default);
    IReadOnlyList<Shift> GetShifts(int pharmacistStaffId, DateTime rangeStart, DateTime rangeEnd);
    Task<IReadOnlyList<Pharmacyst>> GetPharmacistsAsync(CancellationToken cancellationToken = default);
    IReadOnlyList<Pharmacyst> GetPharmacists();
}
