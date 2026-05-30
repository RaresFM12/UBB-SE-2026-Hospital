using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class AppointmentRepository(HospitalDbContext context) : IAppointmentRepository
{
    public async Task<Appointment?> GetByIdAsync(int appointmentId)
        => await context.Appointments.FindAsync(appointmentId);

    public async Task<List<Appointment>> GetAllAsync()
        => await context.Appointments.ToListAsync();

    public async Task<List<Appointment>> GetByDoctorIdAsync(int doctorId)
        => await context.Appointments.Where(a => a.Doctor!.StaffId == doctorId).ToListAsync();

    public async Task<List<Appointment>> GetByPatientIdAsync(int patientId)
        => await context.Appointments.ToListAsync();

    public async Task<List<Appointment>> GetByDateRangeAsync(DateTime start, DateTime end)
        => await context.Appointments
            .Where(a => a.AppointmentDate >= start && a.AppointmentDate <= end)
            .ToListAsync();

    public async Task<Appointment> CreateAsync(Appointment appointment)
    {
        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();
        return appointment;
    }

    public async Task<Appointment> UpdateAsync(Appointment appointment)
    {
        context.Appointments.Update(appointment);
        await context.SaveChangesAsync();
        return appointment;
    }

    public async Task DeleteAsync(int appointmentId)
    {
        var appointment = await context.Appointments.FindAsync(appointmentId);
        if (appointment is not null)
        {
            context.Appointments.Remove(appointment);
            await context.SaveChangesAsync();
        }
    }
}
