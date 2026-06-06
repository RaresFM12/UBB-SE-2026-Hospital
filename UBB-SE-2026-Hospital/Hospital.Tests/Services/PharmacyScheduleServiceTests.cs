using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class PharmacyScheduleServiceTests
{
    private const int PharmacistId = 5;
    private static readonly DateTime RangeStart = new(2026, 6, 1);
    private static readonly DateTime RangeEnd = new(2026, 6, 30);
    private static readonly DateTime InRangeStart = new(2026, 6, 10, 8, 0, 0);
    private static readonly DateTime InRangeEnd = new(2026, 6, 10, 16, 0, 0);
    private static readonly DateTime OutOfRangeStart = new(2026, 7, 10, 8, 0, 0);
    private static readonly DateTime OutOfRangeEnd = new(2026, 7, 10, 16, 0, 0);

    private static (PharmacyScheduleService Service, IShiftRepository Shifts, IStaffRepository Staff) CreateService()
    {
        var shifts = Substitute.For<IShiftRepository>();
        var staff = Substitute.For<IStaffRepository>();
        return (new PharmacyScheduleService(shifts, staff), shifts, staff);
    }

    [TestMethod]
    public async Task GetShiftsAsync_ReturnsOnlyShiftsInRange()
    {
        var (service, shifts, _) = CreateService();
        shifts.GetByStaffIdAsync(PharmacistId).Returns(new List<Shift>
        {
            new() { StartTime = InRangeStart, EndTime = InRangeEnd },
            new() { StartTime = OutOfRangeStart, EndTime = OutOfRangeEnd },
        });

        var result = await service.GetShiftsAsync(PharmacistId, RangeStart, RangeEnd);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetPharmacistsAsync_ReturnsRepositoryResult()
    {
        var (service, _, staff) = CreateService();
        staff.GetAllPharmacistsAsync().Returns(new List<Pharmacyst> { new() { StaffId = PharmacistId } });

        var result = await service.GetPharmacistsAsync();

        Assert.HasCount(1, result);
    }
}
