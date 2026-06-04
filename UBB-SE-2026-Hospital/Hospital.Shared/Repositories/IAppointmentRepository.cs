namespace Hospital.Shared.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Hospital.Shared.Models;

    public interface IAppointmentRepository
    {
        Task<IReadOnlyList<Appointment>> GetAllAppointmentsAsync();

        Task AddAppointmentAsync(int patientId, int doctorId, DateTime startTime, DateTime endTime, string status);

        Task UpdateAppointmentStatusAsync(int appointmentId, string status);
    }
}
