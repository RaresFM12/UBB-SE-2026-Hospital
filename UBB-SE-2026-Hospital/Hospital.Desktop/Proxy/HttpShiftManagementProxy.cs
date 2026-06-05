using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Proxy;

public class HttpShiftManagementProxy(HttpClient httpClient) : ProxyBase(httpClient), IShiftManagementService
{
    private const string BaseUri = "api/shifts";

    public void SetShiftActive(int shiftId)
        => Task.Run(async () => await PutAsync<object>($"{BaseUri}/{shiftId}/status", new { status = "Active" })).GetAwaiter().GetResult();

    public void CancelShift(int shiftId)
        => Task.Run(async () => await PutAsync<object>($"{BaseUri}/{shiftId}/status", new { status = "Cancelled" })).GetAwaiter().GetResult();

    public bool TryAddShift(IStaff staff, DateTime start, DateTime end, string location)
    {
        try
        {
            var shift = new Shift(0, staff, location, start, end, ShiftStatus.Scheduled);
            Task.Run(async () => await PostAsync<Shift, object>(BaseUri, shift)).GetAwaiter().GetResult();
            return true;
        }
        catch { return false; }
    }

    public bool ValidateShiftTimes(TimeSpan start, TimeSpan end) => start < end;

    public IReadOnlyList<Shift> GetDailyShifts(DateTime date)
        => Task.Run(async () => await GetAsync<List<Shift>>($"{BaseUri}/daily?date={date:yyyy-MM-dd}") ?? []).GetAwaiter().GetResult();

    public IReadOnlyList<Shift> GetWeeklyShifts(DateTime date)
        => Task.Run(async () => await GetAsync<List<Shift>>($"{BaseUri}/weekly?date={date:yyyy-MM-dd}") ?? []).GetAwaiter().GetResult();

    public bool ReassignShift(Shift shift, IStaff newStaff)
    {
        try
        {
            Task.Run(async () => await PutAsync<object>($"{BaseUri}/{shift.Id}/staff", new { staffId = newStaff.StaffID })).GetAwaiter().GetResult();
            return true;
        }
        catch { return false; }
    }

    public IReadOnlyList<IStaff> GetFilteredStaff(string location, string requiredSpecializationOrCertification)
        => Task.Run(async () => await GetAsync<List<Staff>>($"api/staff/filtered?location={Uri.EscapeDataString(location)}&requiredSpecializationOrCertification={Uri.EscapeDataString(requiredSpecializationOrCertification)}") ?? [])
              .GetAwaiter().GetResult().Cast<IStaff>().ToList();

    public IReadOnlyList<IStaff> FindStaffReplacements(Shift shift)
        => Task.Run(async () => await GetAsync<List<Staff>>($"{BaseUri}/{shift.Id}/replacements") ?? [])
              .GetAwaiter().GetResult().Cast<IStaff>().ToList();

    public IReadOnlyList<string> GetSpecializationsAndCertificationsForLocation(string location)
        => Task.Run(async () => await GetAsync<List<string>>($"api/staff/specializations?location={Uri.EscapeDataString(location)}") ?? []).GetAwaiter().GetResult();

    public bool IsStaffWorkingDuring(int staffId, DateTime startTime, DateTime endTime)
        => Task.Run(async () => await GetAsync<bool>($"{BaseUri}/is-working?staffId={staffId}&startTime={QueryDate(startTime)}&endTime={QueryDate(endTime)}")).GetAwaiter().GetResult();

    public async Task<IReadOnlyList<Shift>> GetAllShiftsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<Shift>>(BaseUri) ?? [];

    public async Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default)
        => await GetAsync<Shift>($"{BaseUri}/{shiftId}");

    public async Task CreateShiftAsync(int staffId, string location, DateTime startTime, DateTime endTime, ShiftStatus status, CancellationToken cancellationToken = default)
        => await PostAsync<object, object>(BaseUri, new { staffId, location, startTime, endTime, status });

    public async Task UpdateShiftStatusAsync(int shiftId, ShiftStatus status, CancellationToken cancellationToken = default)
        => await PutAsync<object>($"{BaseUri}/{shiftId}/status", new { status });

    public async Task UpdateShiftStaffAsync(int shiftId, int staffId, CancellationToken cancellationToken = default)
        => await PutAsync<object>($"{BaseUri}/{shiftId}/staff", new { staffId });

    public async Task DeleteShiftAsync(int shiftId, CancellationToken cancellationToken = default)
        => await DeleteAsync($"{BaseUri}/{shiftId}");

    public async Task<IReadOnlyList<Shift>> GetDailyShiftsAsync(DateTime date, CancellationToken cancellationToken = default)
        => await GetAsync<List<Shift>>($"{BaseUri}/daily?date={date:yyyy-MM-dd}") ?? [];

    public async Task<IReadOnlyList<Shift>> GetWeeklyShiftsAsync(DateTime date, CancellationToken cancellationToken = default)
        => await GetAsync<List<Shift>>($"{BaseUri}/weekly?date={date:yyyy-MM-dd}") ?? [];

    public async Task<IReadOnlyList<Shift>> GetActiveShiftsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<Shift>>($"{BaseUri}/active") ?? [];

    public async Task<float> GetWeeklyHoursAsync(int staffId, CancellationToken cancellationToken = default)
        => await GetAsync<float>($"{BaseUri}/weekly-hours/{staffId}");

    public async Task<bool> ValidateNoOverlapAsync(int staffId, DateTime start, DateTime end, CancellationToken cancellationToken = default)
        => await GetAsync<bool>($"{BaseUri}/validate-no-overlap?staffId={staffId}&start={QueryDate(start)}&end={QueryDate(end)}");

    public async Task<IReadOnlyList<Staff>> GetAllStaffAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<Staff>>("api/staff") ?? [];

    public async Task<Staff?> GetStaffByIdAsync(int staffId, CancellationToken cancellationToken = default)
        => await GetAsync<Staff>($"api/staff/{staffId}");

    public async Task<IReadOnlyList<Doctor>> GetDoctorsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<Doctor>>("api/staff/doctors") ?? [];

    public async Task<IReadOnlyList<Pharmacyst>> GetPharmacistsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<Pharmacyst>>("api/staff/pharmacists") ?? [];

    public async Task UpdateStaffStatusAsync(int staffId, string status, CancellationToken cancellationToken = default)
        => await PutAsync<object>($"api/staff/{staffId}/status", new { status });

    public async Task UpdateStaffAvailabilityAsync(int staffId, bool isAvailable, DoctorStatus status, CancellationToken cancellationToken = default)
        => await PutAsync<object>($"api/staff/{staffId}/availability", new { isAvailable, status });

    public async Task<IReadOnlyList<Staff>> GetFilteredStaffAsync(string location, string requiredSpecializationOrCertification, CancellationToken cancellationToken = default)
        => await GetAsync<List<Staff>>($"api/staff/filtered?location={Uri.EscapeDataString(location)}&requiredSpecializationOrCertification={Uri.EscapeDataString(requiredSpecializationOrCertification)}") ?? [];
}
