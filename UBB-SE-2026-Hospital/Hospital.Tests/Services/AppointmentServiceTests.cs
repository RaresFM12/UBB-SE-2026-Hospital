using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class AppointmentServiceTests
{
    private const int AppointmentId = 11;
    private const int DoctorId = 3;
    private const string FinishedStatus = "Finished";
    private const string CanceledStatus = "Canceled";
    private const string DoctorEmail = "doc@hospital.test";

    private static (AppointmentService Service, IAppointmentRepository Appointments, IStaffRepository Staff, IShiftRepository Shifts) CreateService()
    {
        var appointments = Substitute.For<IAppointmentRepository>();
        var staff = Substitute.For<IStaffRepository>();
        var shifts = Substitute.For<IShiftRepository>();
        return (new AppointmentService(appointments, staff, shifts), appointments, staff, shifts);
    }

    [TestMethod]
    public async Task UpdateAppointmentStatusAsync_AppointmentNotFound_ThrowsArgumentException()
    {
        var (service, appointments, _, _) = CreateService();
        appointments.GetByIdAsync(AppointmentId).Returns((Appointment?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdateAppointmentStatusAsync(AppointmentId, CanceledStatus));
    }

    [TestMethod]
    public async Task UpdateAppointmentStatusAsync_PersistsNewStatus()
    {
        var (service, appointments, _, _) = CreateService();
        appointments.GetByIdAsync(AppointmentId).Returns(new Appointment { Id = AppointmentId });

        await service.UpdateAppointmentStatusAsync(AppointmentId, CanceledStatus);

        await appointments.Received().UpdateAsync(Arg.Is<Appointment>(appointment => appointment.Status == CanceledStatus));
    }

    [TestMethod]
    public async Task FinishAppointmentAsync_AlreadyFinished_ThrowsInvalidOperationException()
    {
        var (service, appointments, _, _) = CreateService();
        appointments.GetByIdAsync(AppointmentId).Returns(new Appointment { Id = AppointmentId, Status = FinishedStatus });

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.FinishAppointmentAsync(AppointmentId));
    }

    [TestMethod]
    public async Task CancelAppointmentAsync_AlreadyFinished_ThrowsInvalidOperationException()
    {
        var (service, appointments, _, _) = CreateService();
        appointments.GetByIdAsync(AppointmentId).Returns(new Appointment { Id = AppointmentId, Status = FinishedStatus });

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.CancelAppointmentAsync(AppointmentId));
    }

    [TestMethod]
    public async Task GetDoctorIdByEmailAsync_EmptyEmail_ReturnsNull()
    {
        var (service, _, _, _) = CreateService();

        int? doctorId = await service.GetDoctorIdByEmailAsync(string.Empty);

        Assert.IsNull(doctorId);
    }

    [TestMethod]
    public async Task GetDoctorIdByEmailAsync_MatchingDoctor_ReturnsStaffId()
    {
        var (service, _, staff, _) = CreateService();
        staff.GetAllDoctorsAsync().Returns(new List<Doctor> { new() { StaffId = DoctorId, Email = DoctorEmail } });

        int? doctorId = await service.GetDoctorIdByEmailAsync(DoctorEmail);

        Assert.AreEqual(DoctorId, doctorId);
    }

    [TestMethod]
    public async Task GetAppointmentsForDoctorAsync_OrdersByAppointmentDate()
    {
        var (service, appointments, _, _) = CreateService();
        var earlier = new Appointment { Id = 1, AppointmentDate = new DateTime(2024, 1, 1) };
        var later = new Appointment { Id = 2, AppointmentDate = new DateTime(2024, 2, 1) };
        appointments.GetByDoctorIdAsync(DoctorId).Returns(new List<Appointment> { later, earlier });

        var result = await service.GetAppointmentsForDoctorAsync(DoctorId);

        Assert.AreEqual(earlier.Id, result[0].Id);
    }

    [TestMethod]
    public async Task GetAllDoctorsAsync_MapsToIdAndName()
    {
        var (service, _, staff, _) = CreateService();
        staff.GetAllDoctorsAsync().Returns(new List<Doctor> { new() { StaffId = DoctorId, FirstName = "Ana", LastName = "Pop" } });

        var result = await service.GetAllDoctorsAsync();

        Assert.AreEqual(DoctorId, result[0].DoctorId);
    }

    [TestMethod]
    public async Task GetAllAppointmentsAsync_ReturnsRepositoryResult()
    {
        var (service, appointments, _, _) = CreateService();
        appointments.GetAllAsync().Returns(new List<Appointment> { new() { Id = AppointmentId } });

        var result = await service.GetAllAppointmentsAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetAppointmentByIdAsync_ReturnsRepositoryResult()
    {
        var (service, appointments, _, _) = CreateService();
        appointments.GetByIdAsync(AppointmentId).Returns(new Appointment { Id = AppointmentId });

        var result = await service.GetAppointmentByIdAsync(AppointmentId);

        Assert.AreEqual(AppointmentId, result!.Id);
    }

    [TestMethod]
    public async Task GetUpcomingAppointmentsAsync_FiltersByDoctorAndWindow()
    {
        var (service, appointments, _, _) = CreateService();
        var today = DateTime.Today;
        appointments.GetAllAsync().Returns(new List<Appointment>
        {
            new() { Id = 1, Doctor = new Doctor { StaffId = DoctorId }, AppointmentDate = today },
            new() { Id = 2, Doctor = new Doctor { StaffId = DoctorId + 1 }, AppointmentDate = today },
        });

        var result = await service.GetUpcomingAppointmentsAsync(DoctorId, today, 0, 10);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetAppointmentsInRangeAsync_FiltersByRange()
    {
        var (service, appointments, _, _) = CreateService();
        var start = new DateTime(2026, 6, 1);
        appointments.GetByDoctorIdAsync(DoctorId).Returns(new List<Appointment>
        {
            new() { Id = 1, AppointmentDate = start.AddDays(1) },
            new() { Id = 2, AppointmentDate = start.AddDays(40) },
        });

        var result = await service.GetAppointmentsInRangeAsync(DoctorId, start, start.AddDays(10));

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetShiftsForStaffInRangeAsync_FiltersOverlappingShifts()
    {
        var (service, _, _, shifts) = CreateService();
        var start = new DateTime(2026, 6, 1, 8, 0, 0);
        shifts.GetByStaffIdAsync(DoctorId).Returns(new List<Shift>
        {
            new() { StartTime = start, EndTime = start.AddHours(8) },
            new() { StartTime = start.AddDays(10), EndTime = start.AddDays(10).AddHours(8) },
        });

        var result = await service.GetShiftsForStaffInRangeAsync(DoctorId, start, start.AddHours(4));

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task CreateAppointmentAsync_OffDutyDoctor_ThrowsInvalidOperation()
    {
        var (service, _, staff, _) = CreateService();
        staff.GetByIdAsync(DoctorId).Returns(new Doctor { StaffId = DoctorId, DoctorStatus = DoctorStatus.OffDuty });

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.CreateAppointmentAsync(1, DoctorId, DateTime.Today, DateTime.Today.AddHours(1), string.Empty));
    }

    [TestMethod]
    public async Task CreateAppointmentAsync_BookableDoctor_CreatesAppointment()
    {
        var (service, appointments, staff, shifts) = CreateService();
        staff.GetByIdAsync(DoctorId).Returns(new Doctor { StaffId = DoctorId, DoctorStatus = DoctorStatus.Available });
        shifts.GetByStaffIdAsync(DoctorId).Returns(new List<Shift>());

        await service.CreateAppointmentAsync(1, DoctorId, DateTime.Today, DateTime.Today.AddHours(1), string.Empty);

        await appointments.Received().CreateAsync(Arg.Any<Appointment>());
    }

    [TestMethod]
    public async Task BookAppointmentAsync_NoAssignedDoctor_ThrowsInvalidOperation()
    {
        var (service, appointments, _, _) = CreateService();
        appointments.GetByIdAsync(AppointmentId).Returns(new Appointment { Id = AppointmentId, Doctor = null });

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.BookAppointmentAsync(AppointmentId));
    }

    [TestMethod]
    public async Task FinishAppointmentAsync_Valid_SetsFinishedStatus()
    {
        var (service, appointments, _, _) = CreateService();
        appointments.GetByIdAsync(AppointmentId).Returns(new Appointment { Id = AppointmentId, Status = CanceledStatus });

        await service.FinishAppointmentAsync(AppointmentId);

        await appointments.Received().UpdateAsync(Arg.Is<Appointment>(appointment => appointment.Status == FinishedStatus));
    }

    [TestMethod]
    public async Task CancelAppointmentAsync_Valid_SetsCanceledStatus()
    {
        var (service, appointments, _, _) = CreateService();
        appointments.GetByIdAsync(AppointmentId).Returns(new Appointment { Id = AppointmentId, Status = string.Empty });

        await service.CancelAppointmentAsync(AppointmentId);

        await appointments.Received().UpdateAsync(Arg.Is<Appointment>(appointment => appointment.Status == CanceledStatus));
    }

    [TestMethod]
    public async Task GetAppointmentDetailsAsync_ReturnsAppointment()
    {
        var (service, appointments, _, _) = CreateService();
        appointments.GetByIdAsync(AppointmentId).Returns(new Appointment { Id = AppointmentId });

        var result = await service.GetAppointmentDetailsAsync(AppointmentId);

        Assert.AreEqual(AppointmentId, result!.Id);
    }

    [TestMethod]
    public async Task CreateAppointmentAsync_WithinActiveShift_CreatesAppointment()
    {
        var (service, appointments, staff, shifts) = CreateService();
        var shiftStart = new DateTime(2026, 6, 10, 9, 0, 0);
        staff.GetByIdAsync(DoctorId).Returns(new Doctor { StaffId = DoctorId, DoctorStatus = DoctorStatus.Available });
        shifts.GetByStaffIdAsync(DoctorId).Returns(new List<Shift>
        {
            new() { Id = 1, StartTime = shiftStart, EndTime = shiftStart.AddHours(8), Status = ShiftStatus.Scheduled },
        });

        await service.CreateAppointmentAsync(1, DoctorId, shiftStart.AddHours(1), shiftStart.AddHours(2), string.Empty);

        await appointments.Received().CreateAsync(Arg.Any<Appointment>());
    }

    [TestMethod]
    public async Task CreateAppointmentAsync_ExceedsConsecutiveDutyLimit_ThrowsInvalidOperation()
    {
        var (service, _, staff, shifts) = CreateService();
        var shiftStart = new DateTime(2026, 6, 10, 0, 0, 0);
        staff.GetByIdAsync(DoctorId).Returns(new Doctor { StaffId = DoctorId, DoctorStatus = DoctorStatus.Available });
        shifts.GetByStaffIdAsync(DoctorId).Returns(new List<Shift>
        {
            new() { Id = 1, StartTime = shiftStart, EndTime = shiftStart.AddHours(13), Status = ShiftStatus.Scheduled },
        });

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.CreateAppointmentAsync(1, DoctorId, shiftStart.AddHours(12.5), shiftStart.AddHours(13), string.Empty));
    }
}
