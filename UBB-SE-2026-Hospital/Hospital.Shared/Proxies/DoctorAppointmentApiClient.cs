using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class DoctorAppointmentApiClient(HttpClient httpClient) : ApiClientBase(httpClient), IDoctorAppointmentService, IDoctorAppointmentApiClient
{
    private const string BaseUri = "api/appointments";

    public async Task<IReadOnlyList<Appointment>> GetAllAppointmentsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<Appointment>>(BaseUri) ?? [];

    public async Task<IReadOnlyList<Appointment>> GetUpcomingAppointmentsAsync(int doctorUserId, DateTime fromDate, int skipCount, int takeCount, CancellationToken cancellationToken = default)
    {
        var allAppointments = await GetAsync<List<Appointment>>(BaseUri) ?? new List<Appointment>();
        return allAppointments
            .Where(a => a.Doctor != null && a.Doctor.StaffID == doctorUserId && a.AppointmentDate >= fromDate)
            .OrderBy(a => a.AppointmentDate)
            .Skip(skipCount)
            .Take(takeCount)
            .ToList();
    }

    public async Task<IReadOnlyList<Appointment>> GetAppointmentsForDoctorAsync(int doctorId, CancellationToken cancellationToken = default)
        => await GetAsync<List<Appointment>>($"{BaseUri}?doctorId={doctorId}") ?? [];

    public async Task<IReadOnlyList<Appointment>> GetAppointmentsInRangeAsync(int doctorId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var allAppointments = await GetAsync<List<Appointment>>(BaseUri) ?? new List<Appointment>();
        return allAppointments
            .Where(a => a.Doctor != null
                     && a.Doctor.StaffID == doctorId
                     && a.AppointmentDate >= fromDate
                     && a.AppointmentDate <= toDate)
            .OrderBy(a => a.AppointmentDate)
            .ToList();
    }

    public async Task<IReadOnlyList<(int DoctorId, string DoctorName)>> GetAllDoctorsAsync(CancellationToken cancellationToken = default)
    {
        var doctors = await GetAsync<List<DoctorOptionDto>>("api/staff/doctors") ?? [];
        return doctors.Select(d => (d.DoctorId, d.DoctorName)).ToList();
    }

    public async Task<IReadOnlyList<Shift>> GetShiftsForStaffInRangeAsync(int staffId, DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {

        var allShifts = await GetAsync<List<Shift>>("api/shifts") ?? new List<Shift>();

        return allShifts
            .Where(shift => shift.AppointedStaff != null
                         && shift.AppointedStaff.StaffID == staffId
                         && shift.StartTime < end
                         && shift.EndTime > start)
            .OrderBy(shift => shift.StartTime)
            .ToList();
    }
    
    public async Task CreateAppointmentAsync(int patientId, int doctorId, DateTime startTime, DateTime endTime, string status, CancellationToken cancellationToken = default)
        => await PostAsync<object, object>(BaseUri, new { patientId, doctorId, startTime, endTime, status });

    public async Task UpdateAppointmentStatusAsync(int appointmentId, string status, CancellationToken cancellationToken = default)
        => await PutAsync<object>($"{BaseUri}/{appointmentId}/status", new { status });

    public async Task BookAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default)
        => await PutAsync<object>($"{BaseUri}/{appointmentId}/status", new { status = "Booked" });

    public async Task FinishAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default)
        => await PutAsync<object>($"{BaseUri}/{appointmentId}/status", new { status = "Completed" });

    public async Task CancelAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default)
        => await PutAsync<object>($"{BaseUri}/{appointmentId}/status", new { status = "Cancelled" });

    public async Task<int?> GetDoctorIdByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await GetAsync<int?>($"api/staff/doctors/by-email?email={Uri.EscapeDataString(email)}");

    private sealed class DoctorOptionDto
    {
        public int DoctorId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string DoctorName => $"{FirstName} {LastName}";
    }
    public async Task<Appointment?> GetAppointmentByIdAsync(int appointmentId, CancellationToken cancellationToken = default)
        => await GetAsync<Appointment?>($"{BaseUri}/{appointmentId}", cancellationToken);

    public async Task<Appointment?> GetAppointmentDetailsAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        var allAppointments = await GetAsync<List<Appointment>>("api/appointments") ?? [];
        var appointment = allAppointments.FirstOrDefault(a => a.Id == appointmentId);

        if (appointment == null)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG: Appointment with ID {appointmentId} was not found in the list!");
        }

        return appointment;
    }
}


