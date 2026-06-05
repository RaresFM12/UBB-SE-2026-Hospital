using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Proxy;

public class HttpDoctorAppointmentProxy(HttpClient httpClient) : ProxyBase(httpClient), IDoctorAppointmentService
{
    private const string BaseUri = "api/appointments";

    public async Task<IReadOnlyList<Appointment>> GetAllAppointmentsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<Appointment>>(BaseUri) ?? [];

    public async Task<IReadOnlyList<Appointment>> GetUpcomingAppointmentsAsync(int doctorUserId, DateTime fromDate, int skipCount, int takeCount, CancellationToken cancellationToken = default)
        => await GetAsync<List<Appointment>>($"{BaseUri}/upcoming?doctorUserId={doctorUserId}&fromDate={QueryDate(fromDate)}&skipCount={skipCount}&takeCount={takeCount}") ?? [];

    public async Task<IReadOnlyList<Appointment>> GetAppointmentsForDoctorAsync(int doctorId, CancellationToken cancellationToken = default)
        => await GetAsync<List<Appointment>>($"{BaseUri}?doctorId={doctorId}") ?? [];

    public async Task<IReadOnlyList<Appointment>> GetAppointmentsInRangeAsync(int doctorId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        => await GetAsync<List<Appointment>>($"{BaseUri}/range?doctorId={doctorId}&fromDate={QueryDate(fromDate)}&toDate={QueryDate(toDate)}") ?? [];

    public async Task<Appointment?> GetAppointmentByIdAsync(int appointmentId, CancellationToken cancellationToken = default)
        => await GetAsync<Appointment>($"{BaseUri}/{appointmentId}");

    public async Task<IReadOnlyList<(int DoctorId, string DoctorName)>> GetAllDoctorsAsync(CancellationToken cancellationToken = default)
    {
        var doctors = await GetAsync<List<DoctorOptionDto>>("api/doctors") ?? [];
        return doctors.Select(d => (d.DoctorId, d.DoctorName)).ToList();
    }

    public async Task<IReadOnlyList<Shift>> GetShiftsForStaffInRangeAsync(int staffId, DateTime start, DateTime end, CancellationToken cancellationToken = default)
        => await GetAsync<List<Shift>>($"api/shifts/range?staffId={staffId}&fromDate={QueryDate(start)}&toDate={QueryDate(end)}") ?? [];

    public async Task CreateAppointmentAsync(int patientId, int doctorId, DateTime startTime, DateTime endTime, string status, CancellationToken cancellationToken = default)
        => await PostAsync<object, object>(BaseUri, new { patientId, doctorId, startTime, endTime, status });

    public async Task UpdateAppointmentStatusAsync(int appointmentId, string status, CancellationToken cancellationToken = default)
        => await PutAsync<object>($"{BaseUri}/{appointmentId}/status", new { status });

    public async Task BookAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default)
        => await PostAsync<object, object>($"{BaseUri}/{appointmentId}/book", new { });

    public async Task FinishAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default)
        => await PostAsync<object, object>($"{BaseUri}/{appointmentId}/finish", new { });

    public async Task CancelAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default)
        => await PostAsync<object, object>($"{BaseUri}/{appointmentId}/cancel", new { });

    public async Task<int?> GetDoctorIdByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await GetAsync<int?>($"api/doctors/by-email?email={Uri.EscapeDataString(email)}");

    private sealed class DoctorOptionDto
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
    }
    public async Task<Hospital.Data.Models.Appointment?> GetAppointmentDetailsAsync(int appointmentId, CancellationToken cancellationToken = default)
        => await GetAsync<Hospital.Data.Models.Appointment>($"{BaseUri}/{appointmentId}");

}
