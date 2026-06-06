using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class ShiftManagementServiceTests
{
    private const int StaffId = 2;
    private const int ShiftId = 13;
    private const string Location = "ER";
    private const string PharmacyLocation = "Pharmacy";
    private const string Certification = "Clinical";
    private const string Specialization = "Cardiology";
    private const string StaffStatusLabel = "AVAILABLE";
    private const double ExpectedWeeklyHours = 8.0;
    private static readonly DateTime ShiftStart = new(2026, 6, 10, 8, 0, 0);
    private static readonly DateTime ShiftEnd = new(2026, 6, 10, 16, 0, 0);
    private static readonly DateTime CurrentWeekShiftStart = DateTime.Now;
    private static readonly DateTime CurrentWeekShiftEnd = DateTime.Now.AddHours(ExpectedWeeklyHours);

    private static (ShiftManagementService Service, IStaffRepository Staff, IShiftRepository Shifts) CreateService()
    {
        var staff = Substitute.For<IStaffRepository>();
        var shifts = Substitute.For<IShiftRepository>();
        return (new ShiftManagementService(staff, shifts), staff, shifts);
    }

    [TestMethod]
    public async Task CreateShiftAsync_StaffNotFound_ThrowsArgumentException()
    {
        var (service, staff, _) = CreateService();
        staff.GetByIdAsync(StaffId).Returns((Staff?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CreateShiftAsync(StaffId, Location, ShiftStart, ShiftEnd, ShiftStatus.Scheduled));
    }

    [TestMethod]
    public async Task CreateShiftAsync_OverlappingShift_ThrowsInvalidOperationException()
    {
        var (service, staff, shifts) = CreateService();
        staff.GetByIdAsync(StaffId).Returns(new Staff { StaffId = StaffId });
        shifts.GetByStaffIdAsync(StaffId).Returns(new List<Shift>
        {
            new() { StartTime = ShiftStart, EndTime = ShiftEnd, Status = ShiftStatus.Scheduled },
        });

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.CreateShiftAsync(StaffId, Location, ShiftStart, ShiftEnd, ShiftStatus.Scheduled));
    }

    [TestMethod]
    public async Task UpdateShiftStatusAsync_ShiftNotFound_ThrowsArgumentException()
    {
        var (service, _, shifts) = CreateService();
        shifts.GetByIdAsync(ShiftId).Returns((Shift?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdateShiftStatusAsync(ShiftId, ShiftStatus.Cancelled));
    }

    [TestMethod]
    public async Task UpdateStaffStatusAsync_StaffNotFound_ThrowsArgumentException()
    {
        var (service, staff, _) = CreateService();
        staff.GetByIdAsync(StaffId).Returns((Staff?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdateStaffStatusAsync(StaffId, ShiftStatus.Active.ToString()));
    }

    [TestMethod]
    public async Task ValidateNoOverlapAsync_NoExistingShifts_ReturnsTrue()
    {
        var (service, _, shifts) = CreateService();
        shifts.GetByStaffIdAsync(StaffId).Returns(new List<Shift>());

        bool isFree = await service.ValidateNoOverlapAsync(StaffId, ShiftStart, ShiftEnd);

        Assert.IsTrue(isFree);
    }

    [TestMethod]
    public async Task ValidateNoOverlapAsync_OverlappingShift_ReturnsFalse()
    {
        var (service, _, shifts) = CreateService();
        shifts.GetByStaffIdAsync(StaffId).Returns(new List<Shift>
        {
            new() { StartTime = ShiftStart, EndTime = ShiftEnd, Status = ShiftStatus.Scheduled },
        });

        bool isFree = await service.ValidateNoOverlapAsync(StaffId, ShiftStart, ShiftEnd);

        Assert.IsFalse(isFree);
    }

    [TestMethod]
    public void ValidateShiftTimes_EndAfterStart_ReturnsTrue()
    {
        var (service, _, _) = CreateService();

        bool isValid = service.ValidateShiftTimes(ShiftStart.TimeOfDay, ShiftEnd.TimeOfDay);

        Assert.IsTrue(isValid);
    }

    [TestMethod]
    public void ValidateShiftTimes_EndBeforeStart_ReturnsFalse()
    {
        var (service, _, _) = CreateService();

        bool isValid = service.ValidateShiftTimes(ShiftEnd.TimeOfDay, ShiftStart.TimeOfDay);

        Assert.IsFalse(isValid);
    }

    [TestMethod]
    public async Task GetActiveShiftsAsync_ReturnsOnlyActiveShifts()
    {
        var (service, _, shifts) = CreateService();
        shifts.GetAllAsync().Returns(new List<Shift>
        {
            new() { Id = 1, Status = ShiftStatus.Active },
            new() { Id = 2, Status = ShiftStatus.Scheduled },
        });

        var result = await service.GetActiveShiftsAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task CreateShiftAsync_Valid_CreatesShift()
    {
        var (service, staff, shifts) = CreateService();
        staff.GetByIdAsync(StaffId).Returns(new Staff { StaffId = StaffId });
        shifts.GetByStaffIdAsync(StaffId).Returns(new List<Shift>());

        await service.CreateShiftAsync(StaffId, Location, ShiftStart, ShiftEnd, ShiftStatus.Scheduled);

        await shifts.Received().CreateAsync(Arg.Any<Shift>());
    }

    [TestMethod]
    public async Task UpdateShiftStatusAsync_Valid_PersistsStatus()
    {
        var (service, _, shifts) = CreateService();
        shifts.GetByIdAsync(ShiftId).Returns(new Shift { Id = ShiftId });

        await service.UpdateShiftStatusAsync(ShiftId, ShiftStatus.Active);

        await shifts.Received().UpdateAsync(Arg.Is<Shift>(shift => shift.Status == ShiftStatus.Active));
    }

    [TestMethod]
    public async Task UpdateShiftStaffAsync_ShiftNotFound_ThrowsArgumentException()
    {
        var (service, _, shifts) = CreateService();
        shifts.GetByIdAsync(ShiftId).Returns((Shift?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdateShiftStaffAsync(ShiftId, StaffId));
    }

    [TestMethod]
    public async Task UpdateShiftStaffAsync_StaffNotFound_ThrowsArgumentException()
    {
        var (service, staff, shifts) = CreateService();
        shifts.GetByIdAsync(ShiftId).Returns(new Shift { Id = ShiftId, StartTime = ShiftStart, EndTime = ShiftEnd });
        staff.GetByIdAsync(StaffId).Returns((Staff?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdateShiftStaffAsync(ShiftId, StaffId));
    }

    [TestMethod]
    public async Task UpdateShiftStaffAsync_Valid_PersistsShift()
    {
        var (service, staff, shifts) = CreateService();
        shifts.GetByIdAsync(ShiftId).Returns(new Shift { Id = ShiftId, StartTime = ShiftStart, EndTime = ShiftEnd });
        staff.GetByIdAsync(StaffId).Returns(new Staff { StaffId = StaffId });
        shifts.GetByStaffIdAsync(StaffId).Returns(new List<Shift>());

        await service.UpdateShiftStaffAsync(ShiftId, StaffId);

        await shifts.Received().UpdateAsync(Arg.Any<Shift>());
    }

    [TestMethod]
    public async Task UpdateStaffStatusAsync_Valid_PersistsStatus()
    {
        var (service, staff, _) = CreateService();
        staff.GetByIdAsync(StaffId).Returns(new Staff { StaffId = StaffId });

        await service.UpdateStaffStatusAsync(StaffId, StaffStatusLabel);

        await staff.Received().UpdateAsync(Arg.Is<Staff>(member => member.Status == StaffStatusLabel));
    }

    [TestMethod]
    public async Task UpdateStaffAvailabilityAsync_Doctor_PersistsDoctor()
    {
        var (service, staff, _) = CreateService();
        staff.GetByIdAsync(StaffId).Returns(new Doctor { StaffId = StaffId });

        await service.UpdateStaffAvailabilityAsync(StaffId, true, DoctorStatus.Available);

        await staff.Received().UpdateAsync(Arg.Is<Staff>(member => member.Available));
    }

    [TestMethod]
    public async Task DeleteShiftAsync_DelegatesToRepository()
    {
        var (service, _, shifts) = CreateService();

        await service.DeleteShiftAsync(ShiftId);

        await shifts.Received().DeleteAsync(ShiftId);
    }

    [TestMethod]
    public async Task GetAllShiftsAsync_ReturnsRepositoryResult()
    {
        var (service, _, shifts) = CreateService();
        shifts.GetAllAsync().Returns(new List<Shift> { new() { Id = ShiftId } });

        var result = await service.GetAllShiftsAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetShiftByIdAsync_ReturnsRepositoryResult()
    {
        var (service, _, shifts) = CreateService();
        shifts.GetByIdAsync(ShiftId).Returns(new Shift { Id = ShiftId });

        var result = await service.GetShiftByIdAsync(ShiftId);

        Assert.AreEqual(ShiftId, result!.Id);
    }

    [TestMethod]
    public async Task GetDailyShiftsAsync_FiltersByDate()
    {
        var (service, _, shifts) = CreateService();
        shifts.GetAllAsync().Returns(new List<Shift>
        {
            new() { Id = 1, StartTime = ShiftStart, EndTime = ShiftEnd },
            new() { Id = 2, StartTime = ShiftStart.AddDays(5), EndTime = ShiftEnd.AddDays(5) },
        });

        var result = await service.GetDailyShiftsAsync(ShiftStart);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetWeeklyHoursAsync_SumsCurrentWeekHours()
    {
        var (service, _, shifts) = CreateService();
        shifts.GetByStaffIdAsync(StaffId).Returns(new List<Shift>
        {
            new() { StartTime = CurrentWeekShiftStart, EndTime = CurrentWeekShiftEnd },
        });

        float hours = await service.GetWeeklyHoursAsync(StaffId);

        Assert.AreEqual((float)ExpectedWeeklyHours, hours);
    }

    [TestMethod]
    public async Task GetAllStaffAsync_ReturnsRepositoryResult()
    {
        var (service, staff, _) = CreateService();
        staff.GetAllAsync().Returns(new List<Staff> { new() { StaffId = StaffId } });

        var result = await service.GetAllStaffAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetStaffByIdAsync_ReturnsRepositoryResult()
    {
        var (service, staff, _) = CreateService();
        staff.GetByIdAsync(StaffId).Returns(new Staff { StaffId = StaffId });

        var result = await service.GetStaffByIdAsync(StaffId);

        Assert.AreEqual(StaffId, result!.StaffId);
    }

    [TestMethod]
    public async Task GetDoctorsAsync_ReturnsRepositoryResult()
    {
        var (service, staff, _) = CreateService();
        staff.GetAllDoctorsAsync().Returns(new List<Doctor> { new() { StaffId = StaffId } });

        var result = await service.GetDoctorsAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetPharmacistsAsync_ReturnsRepositoryResult()
    {
        var (service, staff, _) = CreateService();
        staff.GetAllPharmacistsAsync().Returns(new List<Pharmacyst> { new() { StaffId = StaffId } });

        var result = await service.GetPharmacistsAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetFilteredStaffAsync_PharmacyLocation_FiltersByCertification()
    {
        var (service, staff, _) = CreateService();
        staff.GetAllAsync().Returns(new List<Staff>
        {
            new Pharmacyst { StaffId = StaffId, Certification = Certification },
            new Doctor { StaffId = StaffId + 1, Specialization = Specialization },
        });

        var result = await service.GetFilteredStaffAsync(PharmacyLocation, Certification);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetFilteredStaffAsync_OtherLocation_FiltersBySpecialization()
    {
        var (service, staff, _) = CreateService();
        staff.GetAllAsync().Returns(new List<Staff>
        {
            new Doctor { StaffId = StaffId, Specialization = Specialization },
            new Pharmacyst { StaffId = StaffId + 1, Certification = Certification },
        });

        var result = await service.GetFilteredStaffAsync(Location, Specialization);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void GetSpecializationsAndCertificationsForLocation_Doctor_ReturnsSpecializations()
    {
        var (service, staff, _) = CreateService();
        staff.GetAllAsync().Returns(new List<Staff> { new Doctor { StaffId = StaffId, Specialization = Specialization } });

        var result = service.GetSpecializationsAndCertificationsForLocation(Location);

        Assert.AreEqual(Specialization, result[0]);
    }

    [TestMethod]
    public void ValidateShiftTimes_EqualTimes_ReturnsFalse()
    {
        var (service, _, _) = CreateService();

        bool isValid = service.ValidateShiftTimes(ShiftStart.TimeOfDay, ShiftStart.TimeOfDay);

        Assert.IsFalse(isValid);
    }

    [TestMethod]
    public async Task GetWeeklyShiftsAsync_FiltersCurrentWeek()
    {
        var (service, _, shifts) = CreateService();
        shifts.GetAllAsync().Returns(new List<Shift> { new() { Id = 1, StartTime = CurrentWeekShiftStart, EndTime = CurrentWeekShiftEnd } });

        var result = await service.GetWeeklyShiftsAsync(CurrentWeekShiftStart);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void CancelShift_SetsCancelledStatus()
    {
        var (service, _, shifts) = CreateService();
        shifts.GetByIdAsync(ShiftId).Returns(new Shift { Id = ShiftId });

        service.CancelShift(ShiftId);

        shifts.Received().UpdateAsync(Arg.Is<Shift>(shift => shift.Status == ShiftStatus.Cancelled));
    }

    [TestMethod]
    public void SetShiftActive_SetsActiveStatus()
    {
        var (service, _, shifts) = CreateService();
        shifts.GetByIdAsync(ShiftId).Returns(new Shift { Id = ShiftId });

        service.SetShiftActive(ShiftId);

        shifts.Received().UpdateAsync(Arg.Is<Shift>(shift => shift.Status == ShiftStatus.Active));
    }

    [TestMethod]
    public void TryAddShift_Valid_ReturnsTrue()
    {
        var (service, staff, shifts) = CreateService();
        staff.GetByIdAsync(StaffId).Returns(new Staff { StaffId = StaffId });
        shifts.GetByStaffIdAsync(StaffId).Returns(new List<Shift>());

        bool added = service.TryAddShift(new Staff { StaffId = StaffId }, ShiftStart, ShiftEnd, Location);

        Assert.IsTrue(added);
    }

    [TestMethod]
    public void GetSpecializationsAndCertificationsForLocation_Pharmacy_ReturnsCertifications()
    {
        var (service, staff, _) = CreateService();
        staff.GetAllAsync().Returns(new List<Staff> { new Pharmacyst { StaffId = StaffId, Certification = Certification } });

        var result = service.GetSpecializationsAndCertificationsForLocation(PharmacyLocation);

        Assert.AreEqual(Certification, result[0]);
    }

    [TestMethod]
    public void FindStaffReplacements_DoctorShift_ExcludesOriginalStaff()
    {
        var (service, staff, _) = CreateService();
        var shift = new Shift { Id = ShiftId, Location = Location, Staff = new Doctor { StaffId = StaffId, Specialization = Specialization } };
        staff.GetAllAsync().Returns(new List<Staff>
        {
            new Doctor { StaffId = StaffId, Specialization = Specialization },
            new Doctor { StaffId = StaffId + 1, Specialization = Specialization },
        });

        var result = service.FindStaffReplacements(shift);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task UpdateStaffAvailabilityAsync_NonDoctor_PersistsStaff()
    {
        var (service, staff, _) = CreateService();
        staff.GetByIdAsync(StaffId).Returns(new Staff { StaffId = StaffId });

        await service.UpdateStaffAvailabilityAsync(StaffId, true, DoctorStatus.Available);

        await staff.Received().UpdateAsync(Arg.Is<Staff>(member => member.Available));
    }
}
