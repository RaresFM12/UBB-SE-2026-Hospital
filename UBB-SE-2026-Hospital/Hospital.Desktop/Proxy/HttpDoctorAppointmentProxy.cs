using Hospital.Shared.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Proxy;

public class HttpDoctorAppointmentProxy(HttpClient httpClient) : ProxyBase(httpClient), IDoctorAppointmentService
{
    private const string BaseUri = "api/appointments";

    public async Task<IReadOnlyList<Appointment>> GetUpcomingAppointmentsAsync(int doctorUserId, DateTime fromDate, int skipCount, int takeCount)
        => await GetAsync<List<Appointment>>($"{BaseUri}/upcoming?doctorUserId={doctorUserId}&fromDate={QueryDate(fromDate)}&skipCount={skipCount}&takeCount={takeCount}") ?? [];

    public async Task<IReadOnlyList<(int DoctorId, string DoctorName)>> GetAllDoctorsAsync()
    {
        var doctors = await GetAsync<List<DoctorOptionDto>>("api/doctors") ?? [];
        return doctors.Select(doctor => (doctor.DoctorId, doctor.DoctorName)).ToList();
    }

    public async Task<Appointment?> GetAppointmentDetailsAsync(int appointmentId)
        => await GetAsync<Appointment>($"{BaseUri}/{appointmentId}");

    public async Task<IReadOnlyList<Appointment>> GetAppointmentsForAdminAsync(int doctorId)
        => await GetAsync<List<Appointment>>($"{BaseUri}/admin?doctorId={doctorId}") ?? [];

    public async Task CreateAppointmentAsync(string patientName, int doctorId, DateTime date, TimeSpan startTime)
        => await PostAsync<object, object>(BaseUri, new { patientName, doctorId, date, startTime });

    public async Task BookAppointmentAsync(Appointment appointment)
        => await PostAsync<Appointment, object>($"{BaseUri}/book", appointment);

    public async Task FinishAppointmentAsync(Appointment appointment)
        => await PostAsync<Appointment, object>($"{BaseUri}/{appointment.Id}/finish", appointment);

    public async Task<IReadOnlyList<Appointment>> GetAppointmentsInRangeAsync(int doctorId, DateTime fromDate, DateTime toDate)
        => await GetAsync<List<Appointment>>($"{BaseUri}/range?doctorId={doctorId}&fromDate={QueryDate(fromDate)}&toDate={QueryDate(toDate)}") ?? [];

    public async Task CancelAppointmentAsync(Appointment appointment)
        => await PostAsync<object, object>($"{BaseUri}/{appointment.Id}/cancel", new { });

    public async Task<IReadOnlyList<Shift>> GetShiftsForStaffInRangeAsync(int doctorId, DateTime fromDate, DateTime toDate)
        => await GetAsync<List<Shift>>($"api/shifts/range?doctorId={doctorId}&fromDate={QueryDate(fromDate)}&toDate={QueryDate(toDate)}") ?? [];

    public async Task<int?> GetDoctorIdByEmailAsync(string email)
        => await GetAsync<int?>($"api/doctors/by-email?email={Uri.EscapeDataString(email)}");

    private sealed class DoctorOptionDto
    {
        public int DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;
    }
}
