using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class ShiftSwapServiceTests
{
    private const int SwapId = 31;
    private const int ShiftId = 12;
    private const int RequesterId = 1;
    private const int ColleagueId = 2;
    private const string InvalidStatus = "NOT_A_STATUS";

    private static readonly DateTime RequestedAt = new(2024, 5, 1);

    private static (ShiftSwapService Service, IStaffRepository Staff, IShiftRepository Shifts, IShiftSwapRepository Swaps, INotificationRepository Notifications) CreateService()
    {
        var staff = Substitute.For<IStaffRepository>();
        var shifts = Substitute.For<IShiftRepository>();
        var swaps = Substitute.For<IShiftSwapRepository>();
        var notifications = Substitute.For<INotificationRepository>();
        return (new ShiftSwapService(staff, shifts, swaps, notifications), staff, shifts, swaps, notifications);
    }

    [TestMethod]
    public async Task CreateShiftSwapRequestAsync_ShiftNotFound_ThrowsArgumentException()
    {
        var (service, _, shifts, _, _) = CreateService();
        shifts.GetByIdAsync(ShiftId).Returns((Shift?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CreateShiftSwapRequestAsync(ShiftId, RequesterId, ColleagueId, RequestedAt, ShiftSwapRequestStatus.PENDING));
    }

    [TestMethod]
    public async Task CreateShiftSwapRequestAsync_RequesterNotFound_ThrowsArgumentException()
    {
        var (service, staff, shifts, _, _) = CreateService();
        shifts.GetByIdAsync(ShiftId).Returns(new Shift { Id = ShiftId });
        staff.GetByIdAsync(RequesterId).Returns((Staff?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CreateShiftSwapRequestAsync(ShiftId, RequesterId, ColleagueId, RequestedAt, ShiftSwapRequestStatus.PENDING));
    }

    [TestMethod]
    public async Task UpdateShiftSwapStatusAsync_NotFound_ThrowsArgumentException()
    {
        var (service, _, _, swaps, _) = CreateService();
        swaps.GetByIdAsync(SwapId).Returns((ShiftSwapRequest?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdateShiftSwapStatusAsync(SwapId, nameof(ShiftSwapRequestStatus.ACCEPTED)));
    }

    [TestMethod]
    public async Task UpdateShiftSwapStatusAsync_InvalidStatus_ThrowsArgumentException()
    {
        var (service, _, _, swaps, _) = CreateService();
        swaps.GetByIdAsync(SwapId).Returns(new ShiftSwapRequest { SwapId = SwapId });

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdateShiftSwapStatusAsync(SwapId, InvalidStatus));
    }

    [TestMethod]
    public async Task AcceptSwapRequestAsync_WrongColleague_ReturnsFalse()
    {
        var (service, _, _, swaps, _) = CreateService();
        swaps.GetByIdAsync(SwapId).Returns(new ShiftSwapRequest { SwapId = SwapId, Colleague = new Staff { StaffId = ColleagueId }, Shift = new Shift() });

        bool accepted = await service.AcceptSwapRequestAsync(SwapId, RequesterId);

        Assert.IsFalse(accepted);
    }

    [TestMethod]
    public async Task RejectSwapRequestAsync_WrongColleague_ReturnsFalse()
    {
        var (service, _, _, swaps, _) = CreateService();
        swaps.GetByIdAsync(SwapId).Returns(new ShiftSwapRequest { SwapId = SwapId, Colleague = new Staff { StaffId = ColleagueId } });

        bool rejected = await service.RejectSwapRequestAsync(SwapId, RequesterId);

        Assert.IsFalse(rejected);
    }

    [TestMethod]
    public async Task GetEligibleSwapColleaguesAsync_ShiftNotFound_ThrowsArgumentException()
    {
        var (service, _, shifts, _, _) = CreateService();
        shifts.GetByIdAsync(ShiftId).Returns((Shift?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.GetEligibleSwapColleaguesAsync(RequesterId, ShiftId));
    }

    [TestMethod]
    public async Task GetEligibleSwapColleaguesAsync_ShiftBelongsToOther_ThrowsInvalidOperationException()
    {
        var (service, _, shifts, _, _) = CreateService();
        shifts.GetByIdAsync(ShiftId).Returns(new Shift { Id = ShiftId, Staff = new Staff { StaffId = ColleagueId } });

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.GetEligibleSwapColleaguesAsync(RequesterId, ShiftId));
    }

    private const string Specialization = "Cardiology";
    private static readonly DateTime FutureShiftStart = DateTime.Now.AddDays(10);
    private static readonly DateTime FutureShiftEnd = DateTime.Now.AddDays(10).AddHours(8);

    [TestMethod]
    public async Task CreateShiftSwapRequestAsync_Valid_ReturnsSwapId()
    {
        var (service, staff, shifts, swaps, _) = CreateService();
        shifts.GetByIdAsync(ShiftId).Returns(new Shift { Id = ShiftId });
        staff.GetByIdAsync(RequesterId).Returns(new Staff { StaffId = RequesterId });
        staff.GetByIdAsync(ColleagueId).Returns(new Staff { StaffId = ColleagueId });
        swaps.CreateAsync(Arg.Any<ShiftSwapRequest>()).Returns(new ShiftSwapRequest { SwapId = SwapId });

        int result = await service.CreateShiftSwapRequestAsync(ShiftId, RequesterId, ColleagueId, RequestedAt, ShiftSwapRequestStatus.PENDING);

        Assert.AreEqual(SwapId, result);
    }

    [TestMethod]
    public async Task UpdateShiftSwapStatusAsync_Valid_PersistsStatus()
    {
        var (service, _, _, swaps, _) = CreateService();
        swaps.GetByIdAsync(SwapId).Returns(new ShiftSwapRequest { SwapId = SwapId });

        await service.UpdateShiftSwapStatusAsync(SwapId, nameof(ShiftSwapRequestStatus.ACCEPTED));

        await swaps.Received().UpdateAsync(Arg.Is<ShiftSwapRequest>(request => request.Status == ShiftSwapRequestStatus.ACCEPTED));
    }

    [TestMethod]
    public async Task AcceptSwapRequestAsync_Valid_ReturnsTrue()
    {
        var (service, _, _, swaps, _) = CreateService();
        swaps.GetByIdAsync(SwapId).Returns(new ShiftSwapRequest
        {
            SwapId = SwapId,
            Colleague = new Staff { StaffId = ColleagueId },
            Requester = new Staff { StaffId = RequesterId },
            Shift = new Shift { Id = ShiftId, Staff = new Staff { StaffId = RequesterId } },
        });

        bool accepted = await service.AcceptSwapRequestAsync(SwapId, ColleagueId);

        Assert.IsTrue(accepted);
    }

    [TestMethod]
    public async Task RejectSwapRequestAsync_Valid_ReturnsTrue()
    {
        var (service, _, _, swaps, _) = CreateService();
        swaps.GetByIdAsync(SwapId).Returns(new ShiftSwapRequest
        {
            SwapId = SwapId,
            Colleague = new Staff { StaffId = ColleagueId },
            Requester = new Staff { StaffId = RequesterId },
        });

        bool rejected = await service.RejectSwapRequestAsync(SwapId, ColleagueId);

        Assert.IsTrue(rejected);
    }

    [TestMethod]
    public async Task GetFutureShiftsForStaffAsync_ReturnsOnlyFutureShifts()
    {
        var (service, _, shifts, _, _) = CreateService();
        shifts.GetByStaffIdAsync(RequesterId).Returns(new List<Shift>
        {
            new() { Id = 1, StartTime = FutureShiftStart, EndTime = FutureShiftEnd },
            new() { Id = 2, StartTime = DateTime.Now.AddDays(-10), EndTime = DateTime.Now.AddDays(-9) },
        });

        var result = await service.GetFutureShiftsForStaffAsync(RequesterId);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetEligibleSwapColleaguesAsync_DoctorRequester_ReturnsMatchingColleagues()
    {
        var (service, staff, shifts, _, _) = CreateService();
        shifts.GetByIdAsync(ShiftId).Returns(new Shift { Id = ShiftId, Staff = new Staff { StaffId = RequesterId }, StartTime = FutureShiftStart, EndTime = FutureShiftEnd });
        staff.GetByIdAsync(RequesterId).Returns(new Doctor { StaffId = RequesterId, Specialization = Specialization });
        staff.GetAllAsync().Returns(new List<Staff> { new Doctor { StaffId = ColleagueId, Specialization = Specialization } });
        shifts.GetAllAsync().Returns(new List<Shift>());

        var result = await service.GetEligibleSwapColleaguesAsync(RequesterId, ShiftId);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetAllShiftSwapRequestsAsync_ReturnsRepositoryResult()
    {
        var (service, _, _, swaps, _) = CreateService();
        swaps.GetAllAsync().Returns(new List<ShiftSwapRequest> { new() { SwapId = SwapId } });

        var result = await service.GetAllShiftSwapRequestsAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetShiftSwapByIdAsync_ReturnsRepositoryResult()
    {
        var (service, _, _, swaps, _) = CreateService();
        swaps.GetByIdAsync(SwapId).Returns(new ShiftSwapRequest { SwapId = SwapId });

        var result = await service.GetShiftSwapByIdAsync(SwapId);

        Assert.AreEqual(SwapId, result!.SwapId);
    }

    [TestMethod]
    public void GetAllDoctors_ReturnsRepositoryResult()
    {
        var (service, staff, _, _, _) = CreateService();
        staff.GetAllDoctorsAsync().Returns(new List<Doctor> { new() { StaffId = RequesterId } });

        var result = service.GetAllDoctors();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void GetIncomingSwapRequests_FiltersPendingForColleague()
    {
        var (service, _, _, swaps, _) = CreateService();
        swaps.GetAllAsync().Returns(new List<ShiftSwapRequest>
        {
            new() { SwapId = SwapId, Colleague = new Staff { StaffId = ColleagueId }, Status = ShiftSwapRequestStatus.PENDING },
            new() { SwapId = SwapId + 1, Colleague = new Staff { StaffId = ColleagueId }, Status = ShiftSwapRequestStatus.ACCEPTED },
        });

        var result = service.GetIncomingSwapRequests(ColleagueId);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void RequestShiftSwap_Valid_SetsMessage()
    {
        var (service, staff, shifts, swaps, _) = CreateService();
        shifts.GetByIdAsync(ShiftId).Returns(new Shift { Id = ShiftId });
        staff.GetByIdAsync(RequesterId).Returns(new Staff { StaffId = RequesterId });
        staff.GetByIdAsync(ColleagueId).Returns(new Staff { StaffId = ColleagueId });
        swaps.CreateAsync(Arg.Any<ShiftSwapRequest>()).Returns(new ShiftSwapRequest { SwapId = SwapId });

        service.RequestShiftSwap(RequesterId, ShiftId, ColleagueId, out string message);

        Assert.IsFalse(string.IsNullOrEmpty(message));
    }

    [TestMethod]
    public void GetAllShiftSwapRequests_ReturnsRepositoryResult()
    {
        var (service, _, _, swaps, _) = CreateService();
        swaps.GetAllAsync().Returns(new List<ShiftSwapRequest> { new() { SwapId = SwapId } });

        var result = service.GetAllShiftSwapRequests();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void GetFutureShiftsForStaff_ReturnsFutureShifts()
    {
        var (service, _, shifts, _, _) = CreateService();
        shifts.GetByStaffIdAsync(RequesterId).Returns(new List<Shift> { new() { Id = 1, StartTime = FutureShiftStart, EndTime = FutureShiftEnd } });

        var result = service.GetFutureShiftsForStaff(RequesterId);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void AcceptSwapRequest_Valid_SetsAcceptedMessage()
    {
        var (service, _, _, swaps, _) = CreateService();
        swaps.GetByIdAsync(SwapId).Returns(new ShiftSwapRequest
        {
            SwapId = SwapId,
            Colleague = new Staff { StaffId = ColleagueId },
            Requester = new Staff { StaffId = RequesterId },
            Shift = new Shift { Id = ShiftId, Staff = new Staff { StaffId = RequesterId } },
        });

        service.AcceptSwapRequest(SwapId, ColleagueId, out string message);

        Assert.IsFalse(string.IsNullOrEmpty(message));
    }

    [TestMethod]
    public void RejectSwapRequest_Valid_SetsRejectedMessage()
    {
        var (service, _, _, swaps, _) = CreateService();
        swaps.GetByIdAsync(SwapId).Returns(new ShiftSwapRequest
        {
            SwapId = SwapId,
            Colleague = new Staff { StaffId = ColleagueId },
            Requester = new Staff { StaffId = RequesterId },
        });

        service.RejectSwapRequest(SwapId, ColleagueId, out string message);

        Assert.IsFalse(string.IsNullOrEmpty(message));
    }

    [TestMethod]
    public void GetEligibleSwapColleaguesForShift_DoctorRequester_ReturnsColleagues()
    {
        var (service, staff, shifts, _, _) = CreateService();
        shifts.GetByIdAsync(ShiftId).Returns(new Shift { Id = ShiftId, Staff = new Staff { StaffId = RequesterId }, StartTime = FutureShiftStart, EndTime = FutureShiftEnd });
        staff.GetByIdAsync(RequesterId).Returns(new Doctor { StaffId = RequesterId, Specialization = Specialization });
        staff.GetAllAsync().Returns(new List<Staff> { new Doctor { StaffId = ColleagueId, Specialization = Specialization } });
        shifts.GetAllAsync().Returns(new List<Shift>());

        var result = service.GetEligibleSwapColleaguesForShift(RequesterId, ShiftId, out _);

        Assert.HasCount(1, result);
    }
}
