using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class AppointmentService : IDoctorAppointmentService
{
    public Task<IReadOnlyList<Appointment>> GetAllAppointmentsAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<Appointment>> GetUpcomingAppointmentsAsync(int doctorUserId, DateTime fromDate, int skipCount, int takeCount, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<Appointment>> GetAppointmentsForDoctorAsync(int doctorId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<Appointment>> GetAppointmentsInRangeAsync(int doctorId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<Appointment?> GetAppointmentByIdAsync(int appointmentId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task CreateAppointmentAsync(int patientId, int doctorId, DateTime startTime, DateTime endTime, string status, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task UpdateAppointmentStatusAsync(int appointmentId, string status, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task BookAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task FinishAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task CancelAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<int?> GetDoctorIdByEmailAsync(string email, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
