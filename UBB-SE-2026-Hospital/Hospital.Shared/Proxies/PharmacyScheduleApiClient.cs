using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class PharmacyScheduleApiClient(HttpClient httpClient) : ApiClientBase(httpClient), IPharmacyScheduleService, IPharmacyScheduleApiClient
{
    private const string StaffUri = "api/staff";
    private const string ShiftsUri = "api/shifts";
    private const string PharmacistsUri = "api/staff/pharmacists";

    public async Task<IReadOnlyList<Shift>> GetShiftsAsync(int pharmacistStaffId, DateTime rangeStart, DateTime rangeEnd, CancellationToken cancellationToken = default)
    {
        List<Shift> shifts = await GetAsync<List<Shift>>(ShiftsUri, cancellationToken) ?? [];
        return shifts
            .Where(shift => shift.Staff?.StaffId == pharmacistStaffId && shift.StartTime >= rangeStart && shift.StartTime <= rangeEnd)
            .ToList();
    }

    public IReadOnlyList<Shift> GetShifts(int pharmacistStaffId, DateTime rangeStart, DateTime rangeEnd)
        => Task.Run(async () => (await GetShiftsAsync(pharmacistStaffId, rangeStart, rangeEnd)).ToList()).GetAwaiter().GetResult();

    public async Task<IReadOnlyList<Pharmacyst>> GetPharmacistsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<Pharmacyst>>(PharmacistsUri, cancellationToken) ?? [];

    public IReadOnlyList<Pharmacyst> GetPharmacists()
        => Task.Run(async () => await GetAsync<List<Pharmacyst>>(PharmacistsUri) ?? []).GetAwaiter().GetResult();
}
