using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class AppointmentService(
    IAppointmentRepository appointmentRepository,
    IStaffRepository staffRepository,
    IShiftRepository shiftRepository) : IDoctorAppointmentService
{
    private const int UpcomingAppointmentsWindowDays = 31;
    private const double MaxConsecutiveHours = 12.0;
    private const string ScheduledStatus = "Scheduled";
    private const string FinishedStatus = "Finished";
    private const string CanceledStatus = "Canceled";

    public async Task<IReadOnlyList<Appointment>> GetAllAppointmentsAsync(CancellationToken cancellationToken = default)
        => await appointmentRepository.GetAllAsync();

    public async Task<IReadOnlyList<Appointment>> GetUpcomingAppointmentsAsync(int doctorUserId, DateTime fromDate, int skipCount, int takeCount, CancellationToken cancellationToken = default)
    {
        DateTime from = fromDate.Date;
        DateTime to = from.AddDays(UpcomingAppointmentsWindowDays);

        return (await appointmentRepository.GetAllAsync())
            .Where(appointment => appointment.Doctor?.StaffId == doctorUserId)
            .Where(appointment => appointment.AppointmentDate >= from && appointment.AppointmentDate < to)
            .OrderBy(appointment => appointment.AppointmentDate)
            .Skip(skipCount)
            .Take(takeCount)
            .ToList();
    }

    public async Task<IReadOnlyList<Appointment>> GetAppointmentsForDoctorAsync(int doctorId, CancellationToken cancellationToken = default)
        => (await appointmentRepository.GetByDoctorIdAsync(doctorId))
            .OrderBy(appointment => appointment.AppointmentDate)
            .ToList();

    public async Task<IReadOnlyList<Appointment>> GetAppointmentsInRangeAsync(int doctorId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        => (await appointmentRepository.GetByDoctorIdAsync(doctorId))
            .Where(appointment => appointment.AppointmentDate >= fromDate && appointment.AppointmentDate <= toDate)
            .OrderBy(appointment => appointment.AppointmentDate)
            .ToList();

    public async Task<Appointment?> GetAppointmentByIdAsync(int appointmentId, CancellationToken cancellationToken = default)
        => await appointmentRepository.GetByIdAsync(appointmentId);

    public Task<Appointment?> GetAppointmentDetailsAsync(int appointmentId, CancellationToken cancellationToken = default)
        => GetAppointmentByIdAsync(appointmentId, cancellationToken);

    public async Task<IReadOnlyList<(int DoctorId, string DoctorName)>> GetAllDoctorsAsync(CancellationToken cancellationToken = default)
        => (await staffRepository.GetAllDoctorsAsync())
            .Select(doctor => (doctor.StaffId, doctor.FullName))
            .ToList();

    public async Task<IReadOnlyList<Shift>> GetShiftsForStaffInRangeAsync(int staffId, DateTime start, DateTime end, CancellationToken cancellationToken = default)
        => (await shiftRepository.GetByStaffIdAsync(staffId))
            .Where(shift => shift.StartTime < end && shift.EndTime > start)
            .OrderBy(shift => shift.StartTime)
            .ToList();

    public async Task CreateAppointmentAsync(int patientId, int doctorId, DateTime startTime, DateTime endTime, string status, CancellationToken cancellationToken = default)
    {
        await EnsureDoctorIsBookableAsync(doctorId, startTime);

        await appointmentRepository.CreateAsync(new Appointment
        {
            PatientName = $"PAT-{patientId}",
            ExternalRefId = patientId,
            Doctor = await GetDoctorAsync(doctorId),
            AppointmentDate = startTime.Date,
            StartTime = startTime.TimeOfDay,
            EndTime = endTime.TimeOfDay,
            Status = string.IsNullOrWhiteSpace(status) ? ScheduledStatus : status,
        });
    }

    public async Task UpdateAppointmentStatusAsync(int appointmentId, string status, CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId)
            ?? throw new ArgumentException("Appointment not found.");

        appointment.Status = status;
        await appointmentRepository.UpdateAsync(appointment);
    }

    public async Task BookAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId)
            ?? throw new ArgumentException("Appointment not found.");

        if (appointment.Doctor is null)
        {
            throw new InvalidOperationException("Appointment has no assigned doctor.");
        }

        await EnsureDoctorIsBookableAsync(appointment.Doctor.StaffId, appointment.AppointmentDate);
        appointment.Status = ScheduledStatus;
        await appointmentRepository.UpdateAsync(appointment);
    }

    public async Task FinishAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId)
            ?? throw new ArgumentException("Appointment not found.");

        if (string.Equals(appointment.Status, FinishedStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("This appointment is already finished.");
        }

        appointment.Status = FinishedStatus;
        await appointmentRepository.UpdateAsync(appointment);
    }

    public async Task CancelAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId)
            ?? throw new ArgumentException("Appointment not found.");

        if (string.Equals(appointment.Status, FinishedStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Cannot cancel an appointment that is already Finished.");
        }

        appointment.Status = CanceledStatus;
        await appointmentRepository.UpdateAsync(appointment);
    }

    public async Task<int?> GetDoctorIdByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var doctors = await staffRepository.GetAllDoctorsAsync();
        return doctors.FirstOrDefault(doctor => string.Equals(doctor.Email, email, StringComparison.OrdinalIgnoreCase))?.StaffId;
    }

    private async Task<Doctor> GetDoctorAsync(int doctorId)
        => await staffRepository.GetByIdAsync(doctorId) as Doctor
            ?? throw new ArgumentException($"Doctor #{doctorId} not found.");

    private async Task EnsureDoctorIsBookableAsync(int doctorId, DateTime appointmentStart)
    {
        var doctor = await GetDoctorAsync(doctorId);
        if (doctor.DoctorStatus == DoctorStatus.OffDuty)
        {
            throw new InvalidOperationException($"Doctor #{doctorId} is off duty and cannot accept new appointments.");
        }

        var shifts = await shiftRepository.GetByStaffIdAsync(doctorId);
        var activeShift = shifts.FirstOrDefault(shift =>
            shift.StartTime <= appointmentStart &&
            shift.EndTime >= appointmentStart &&
            shift.Status != ShiftStatus.Cancelled);

        if (activeShift is null)
        {
            return;
        }

        var orderedShifts = shifts
            .Where(shift => shift.Status != ShiftStatus.Cancelled)
            .OrderBy(shift => shift.StartTime)
            .ToList();

        int currentIndex = orderedShifts.FindIndex(shift => shift.Id == activeShift.Id);
        DateTime blockStart = activeShift.StartTime;

        for (int index = currentIndex - 1; index >= 0; index--)
        {
            var previous = orderedShifts[index];
            if (previous.EndTime < blockStart)
            {
                break;
            }

            blockStart = previous.StartTime < blockStart ? previous.StartTime : blockStart;
        }

        if ((appointmentStart - blockStart).TotalHours >= MaxConsecutiveHours)
        {
            throw new InvalidOperationException($"Doctor #{doctorId} exceeded the {MaxConsecutiveHours:F0}h consecutive duty limit.");
        }
    }
}
