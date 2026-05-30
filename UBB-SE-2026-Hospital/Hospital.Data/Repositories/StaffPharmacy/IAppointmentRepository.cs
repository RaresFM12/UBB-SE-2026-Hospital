using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(int appointmentId);
    Task<List<Appointment>> GetAllAsync();
    Task<List<Appointment>> GetByDoctorIdAsync(int doctorId);
    Task<List<Appointment>> GetByPatientIdAsync(int patientId);
    Task<List<Appointment>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<Appointment> CreateAsync(Appointment appointment);
    Task<Appointment> UpdateAsync(Appointment appointment);
    Task DeleteAsync(int appointmentId);
}
