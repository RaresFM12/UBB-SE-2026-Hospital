using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class PharmacyScheduleApiClient(HttpClient httpClient) : ApiClientBase(httpClient), IPharmacyScheduleService
{
    private const string StaffUri = "api/staff";
    private const string ShiftsUri = "api/shifts";

    public async Task<IReadOnlyList<Shift>> GetShiftsAsync(int pharmacistStaffId, DateTime rangeStart, DateTime rangeEnd, CancellationToken cancellationToken = default)
    {
        var allShifts = await GetAsync<List<Shift>>(ShiftsUri) ?? new List<Shift>();
        return allShifts
            .Where(shiftItem => shiftItem.AppointedStaff.StaffID == pharmacistStaffId &&
                                shiftItem.StartTime >= rangeStart &&
                                shiftItem.StartTime <= rangeEnd)
            .ToList();
    }

    public IReadOnlyList<Shift> GetShifts(int pharmacistStaffId, DateTime rangeStart, DateTime rangeEnd)
        => Task.Run(async () => await GetShiftsAsync(pharmacistStaffId, rangeStart, rangeEnd)).GetAwaiter().GetResult();

    public async Task<IReadOnlyList<Pharmacyst>> GetPharmacistsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<Pharmacyst>>($"{StaffUri}/pharmacists") ?? new List<Pharmacyst>();

    public IReadOnlyList<Pharmacyst> GetPharmacists()
        => Task.Run(async () => await GetPharmacistsAsync()).GetAwaiter().GetResult();
}
