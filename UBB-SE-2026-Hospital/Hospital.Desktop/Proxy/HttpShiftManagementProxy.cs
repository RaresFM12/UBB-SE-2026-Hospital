using Hospital.Shared.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Proxy;

public class HttpShiftManagementProxy(HttpClient httpClient) : ProxyBase(httpClient), IShiftManagementService
{
    private const string BaseUri = "api/shifts";

    public void SetShiftActive(int shiftId)
        => Task.Run(async () => await PutAsync<object>($"{BaseUri}/{shiftId}/status", new { status = "Active" })).GetAwaiter().GetResult();

    public void CancelShift(int shiftId)
        => Task.Run(async () => await PutAsync<object>($"{BaseUri}/{shiftId}/status", new { status = "Cancelled" })).GetAwaiter().GetResult();

    public bool ValidateNoOverlap(int staffId, DateTime start, DateTime end)
        => Task.Run(async () => await GetAsync<bool>($"{BaseUri}/validate-no-overlap?staffId={staffId}&start={start:O}&end={end:O}")).GetAwaiter().GetResult();

    public void AddShift(Shift shift)
        => Task.Run(async () => await PostAsync<Shift, object>(BaseUri, shift)).GetAwaiter().GetResult();

    public bool TryAddShift(IStaff staff, DateTime start, DateTime end, string location)
    {
        try
        {
            var shift = new Shift(0, staff, location, start, end, ShiftStatus.SCHEDULED);
            AddShift(shift);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateShiftTimes(TimeSpan start, TimeSpan end)
        => start < end;

    public List<Shift> GetDailyShifts(DateTime date)
        => Task.Run(async () => await GetAsync<List<Shift>>($"{BaseUri}/daily?date={date:yyyy-MM-dd}") ?? []).GetAwaiter().GetResult();

    public List<Shift> GetWeeklyShifts(DateTime date)
        => Task.Run(async () => await GetAsync<List<Shift>>($"{BaseUri}/weekly?date={date:yyyy-MM-dd}") ?? []).GetAwaiter().GetResult();

    public bool ReassignShift(Shift shift, IStaff newStaff)
    {
        try
        {
            Task.Run(async () => await PutAsync<object>($"{BaseUri}/{shift.Id}/staff", new { staffId = newStaff.StaffID })).GetAwaiter().GetResult();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public List<IStaff> GetFilteredStaff(string location, string requiredSpecializationOrCertification)
        => Task.Run(async () => await GetAsync<List<Staff>>($"api/staff/filtered?location={Uri.EscapeDataString(location)}&requiredSpecializationOrCertification={Uri.EscapeDataString(requiredSpecializationOrCertification)}") ?? []).GetAwaiter().GetResult().Cast<IStaff>().ToList();

    public List<IStaff> FindStaffReplacements(Shift shift)
        => Task.Run(async () => await GetAsync<List<Staff>>($"{BaseUri}/{shift.Id}/replacements") ?? []).GetAwaiter().GetResult().Cast<IStaff>().ToList();

    public List<string> GetSpecializationsAndCertificationsForLocation(string location)
        => Task.Run(async () => await GetAsync<List<string>>($"api/staff/specializations?location={Uri.EscapeDataString(location)}") ?? []).GetAwaiter().GetResult();

    public float GetWeeklyHours(int staffId)
        => Task.Run(async () => await GetAsync<float>($"{BaseUri}/weekly-hours/{staffId}")).GetAwaiter().GetResult();

    public List<Shift> GetActiveShifts()
        => Task.Run(async () => await GetAsync<List<Shift>>($"{BaseUri}/active") ?? []).GetAwaiter().GetResult();

    public bool IsStaffWorkingDuring(int staffId, DateTime startTime, DateTime endTime)
        => Task.Run(async () => await GetAsync<bool>($"{BaseUri}/is-working?staffId={staffId}&startTime={startTime:O}&endTime={endTime:O}")).GetAwaiter().GetResult();
}
