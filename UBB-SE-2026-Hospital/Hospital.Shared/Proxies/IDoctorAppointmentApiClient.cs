using Hospital.Data.Models;

namespace Hospital.Shared.Proxies
{
    public interface IDoctorAppointmentApiClient
    {
        Task BookAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default);
        Task CancelAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default);
        Task CreateAppointmentAsync(int patientId, int doctorId, DateTime startTime, DateTime endTime, string status, CancellationToken cancellationToken = default);
        Task FinishAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Appointment>> GetAllAppointmentsAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<(int DoctorId, string DoctorName)>> GetAllDoctorsAsync(CancellationToken cancellationToken = default);
        Task<Appointment?> GetAppointmentByIdAsync(int appointmentId, CancellationToken cancellationToken = default);
        Task<Appointment?> GetAppointmentDetailsAsync(int appointmentId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Appointment>> GetAppointmentsForDoctorAsync(int doctorId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Appointment>> GetAppointmentsInRangeAsync(int doctorId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
        Task<int?> GetDoctorIdByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Shift>> GetShiftsForStaffInRangeAsync(int staffId, DateTime start, DateTime end, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Appointment>> GetUpcomingAppointmentsAsync(int doctorUserId, DateTime fromDate, int skipCount, int takeCount, CancellationToken cancellationToken = default);
        Task UpdateAppointmentStatusAsync(int appointmentId, string status, CancellationToken cancellationToken = default);
    }
}