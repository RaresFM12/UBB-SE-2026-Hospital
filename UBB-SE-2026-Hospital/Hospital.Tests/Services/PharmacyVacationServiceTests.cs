using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class PharmacyVacationServiceTests
{
    private const int PharmacistId = 5;
    private static readonly DateTime StartDate = new(2026, 6, 10);
    private static readonly DateTime EndDate = new(2026, 6, 15);
    private static readonly DateTime EarlierEndDate = new(2026, 6, 5);

    private static (PharmacyVacationService Service, IStaffRepository Staff, IShiftRepository Shifts) CreateService()
    {
        var staff = Substitute.For<IStaffRepository>();
        var shifts = Substitute.For<IShiftRepository>();
        return (new PharmacyVacationService(staff, shifts), staff, shifts);
    }

    [TestMethod]
    public void Constructor_NullStaffRepository_ThrowsArgumentNullException()
    {
        var shifts = Substitute.For<IShiftRepository>();

        Assert.ThrowsExactly<ArgumentNullException>(() => new PharmacyVacationService(null!, shifts));
    }

    [TestMethod]
    public async Task RegisterVacationAsync_EndBeforeStart_ThrowsArgumentException()
    {
        var (service, _, _) = CreateService();

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.RegisterVacationAsync(PharmacistId, StartDate, EarlierEndDate));
    }

    [TestMethod]
    public async Task RegisterVacationAsync_PharmacistNotFound_ThrowsArgumentException()
    {
        var (service, staff, _) = CreateService();
        staff.GetAllPharmacistsAsync().Returns(new List<Pharmacyst>());

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.RegisterVacationAsync(PharmacistId, StartDate, EndDate));
    }

    [TestMethod]
    public async Task RegisterVacationAsync_OverlappingShift_ThrowsInvalidOperationException()
    {
        var (service, staff, shifts) = CreateService();
        staff.GetAllPharmacistsAsync().Returns(new List<Pharmacyst> { new() { StaffId = PharmacistId } });
        shifts.GetByStaffIdAsync(PharmacistId).Returns(new List<Shift>
        {
            new() { StartTime = StartDate, EndTime = EndDate, Status = ShiftStatus.Scheduled },
        });

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.RegisterVacationAsync(PharmacistId, StartDate, EndDate));
    }

    [TestMethod]
    public async Task RegisterVacationAsync_NoConflict_CreatesVacationShift()
    {
        var (service, staff, shifts) = CreateService();
        staff.GetAllPharmacistsAsync().Returns(new List<Pharmacyst> { new() { StaffId = PharmacistId } });
        shifts.GetByStaffIdAsync(PharmacistId).Returns(new List<Shift>());

        await service.RegisterVacationAsync(PharmacistId, StartDate, EndDate);

        await shifts.Received().CreateAsync(Arg.Is<Shift>(shift => shift.Status == ShiftStatus.Vacation));
    }

    [TestMethod]
    public async Task GetPharmacistsAsync_OrdersByName()
    {
        var (service, staff, _) = CreateService();
        staff.GetAllPharmacistsAsync().Returns(new List<Pharmacyst>
        {
            new() { StaffId = PharmacistId, FirstName = "Bea" },
            new() { StaffId = PharmacistId + 1, FirstName = "Ana" },
        });

        var result = await service.GetPharmacistsAsync();

        Assert.AreEqual("Ana", result[0].FirstName);
    }

    [TestMethod]
    public void GetPharmacists_ReturnsRepositoryResult()
    {
        var (service, staff, _) = CreateService();
        staff.GetAllPharmacistsAsync().Returns(new List<Pharmacyst> { new() { StaffId = PharmacistId } });

        var result = service.GetPharmacists();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void RegisterVacation_NoConflict_CreatesVacationShift()
    {
        var (service, staff, shifts) = CreateService();
        staff.GetAllPharmacistsAsync().Returns(new List<Pharmacyst> { new() { StaffId = PharmacistId } });
        shifts.GetByStaffIdAsync(PharmacistId).Returns(new List<Shift>());

        service.RegisterVacation(PharmacistId, StartDate, EndDate);

        shifts.Received().CreateAsync(Arg.Is<Shift>(shift => shift.Status == ShiftStatus.Vacation));
    }

    [TestMethod]
    public async Task RegisterVacationAsync_OverlapsExistingVacation_ThrowsInvalidOperation()
    {
        var (service, staff, shifts) = CreateService();
        staff.GetAllPharmacistsAsync().Returns(new List<Pharmacyst> { new() { StaffId = PharmacistId } });
        shifts.GetByStaffIdAsync(PharmacistId).Returns(new List<Shift>
        {
            new() { StartTime = StartDate, EndTime = EndDate, Status = ShiftStatus.Vacation },
        });

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.RegisterVacationAsync(PharmacistId, StartDate, EndDate));
    }
}
