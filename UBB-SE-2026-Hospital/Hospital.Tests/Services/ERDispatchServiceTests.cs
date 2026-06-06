using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class ERDispatchServiceTests
{
    private const int RequestId = 21;
    private const int DoctorId = 8;
    private const int NearEndMinutes = 30;
    private const int SimulatedCount = 4;
    private const string Specialization = "Surgery";
    private const string Location = "ER";
    private const string AssignedStatus = "ASSIGNED";

    private static (ERDispatchService Service, IERDispatchRepository Dispatch, IStaffRepository Staff, IShiftRepository Shifts, INotificationRepository Notifications) CreateService()
    {
        var dispatch = Substitute.For<IERDispatchRepository>();
        var staff = Substitute.For<IStaffRepository>();
        var shifts = Substitute.For<IShiftRepository>();
        var notifications = Substitute.For<INotificationRepository>();
        return (new ERDispatchService(dispatch, staff, shifts, notifications), dispatch, staff, shifts, notifications);
    }

    [TestMethod]
    public async Task GetRequestByVisitIdAsync_ReturnsNull()
    {
        var (service, _, _, _, _) = CreateService();

        var result = await service.GetRequestByVisitIdAsync(RequestId);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CreateRequestAsync_EmptyStatus_DefaultsToPending()
    {
        var (service, dispatch, _, _, _) = CreateService();
        dispatch.CreateAsync(Arg.Any<ERRequest>()).Returns(call => (ERRequest)call[0]);

        await service.CreateRequestAsync(Specialization, Location, string.Empty);

        await dispatch.Received().CreateAsync(Arg.Is<ERRequest>(request => request.Status == ERRequest.PendingStatus));
    }

    [TestMethod]
    public async Task UpdateRequestStatusAsync_RequestNotFound_ThrowsArgumentException()
    {
        var (service, dispatch, _, _, _) = CreateService();
        dispatch.GetByIdAsync(RequestId).Returns((ERRequest?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdateRequestStatusAsync(RequestId, AssignedStatus));
    }

    [TestMethod]
    public async Task DispatchERRequestAsync_RequestNotFound_ReturnsFailure()
    {
        var (service, dispatch, _, _, _) = CreateService();
        dispatch.GetByIdAsync(RequestId).Returns((ERRequest?)null);

        var result = await service.DispatchERRequestAsync(RequestId);

        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public async Task DispatchERRequestAsync_NoMatchingDoctor_MarksRequestUnmatched()
    {
        var (service, dispatch, staff, shifts, _) = CreateService();
        dispatch.GetByIdAsync(RequestId).Returns(new ERRequest { Id = RequestId, Specialization = Specialization, Location = Location });
        staff.GetAllDoctorsAsync().Returns(new List<Doctor>());
        shifts.GetCurrentShiftsAsync().Returns(new List<Shift>());

        var result = await service.DispatchERRequestAsync(RequestId);

        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public async Task ManualOverrideAsync_RequestNotFound_ThrowsArgumentException()
    {
        var (service, dispatch, _, _, _) = CreateService();
        dispatch.GetByIdAsync(RequestId).Returns((ERRequest?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.ManualOverrideAsync(RequestId, DoctorId, NearEndMinutes));
    }

    [TestMethod]
    public async Task ManualOverrideAsync_DoctorNotFound_ThrowsArgumentException()
    {
        var (service, dispatch, staff, _, _) = CreateService();
        dispatch.GetByIdAsync(RequestId).Returns(new ERRequest { Id = RequestId });
        staff.GetByIdAsync(DoctorId).Returns((Staff?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.ManualOverrideAsync(RequestId, DoctorId, NearEndMinutes));
    }

    [TestMethod]
    public async Task SimulateIncomingRequestsAsync_CreatesRequestedCount()
    {
        var (service, dispatch, _, _, _) = CreateService();
        dispatch.CreateAsync(Arg.Any<ERRequest>()).Returns(call => (ERRequest)call[0]);

        var ids = await service.SimulateIncomingRequestsAsync(SimulatedCount);

        Assert.HasCount(SimulatedCount, ids);
    }

    private const string DoctorName = "Ana Pop";
    private static readonly DateTime ShiftEndNearNow = DateTime.UtcNow.AddMinutes(10);

    [TestMethod]
    public async Task GetAllRequestsAsync_ReturnsRepositoryResult()
    {
        var (service, dispatch, _, _, _) = CreateService();
        dispatch.GetAllAsync().Returns(new List<ERRequest> { new() { Id = RequestId } });

        var result = await service.GetAllRequestsAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetRequestByIdAsync_ReturnsRepositoryResult()
    {
        var (service, dispatch, _, _, _) = CreateService();
        dispatch.GetByIdAsync(RequestId).Returns(new ERRequest { Id = RequestId });

        var result = await service.GetRequestByIdAsync(RequestId);

        Assert.AreEqual(RequestId, result!.Id);
    }

    [TestMethod]
    public async Task CreateRequestAsync_ReturnsCreatedId()
    {
        var (service, dispatch, _, _, _) = CreateService();
        dispatch.CreateAsync(Arg.Any<ERRequest>()).Returns(new ERRequest { Id = RequestId });

        int result = await service.CreateRequestAsync(Specialization, Location);

        Assert.AreEqual(RequestId, result);
    }

    [TestMethod]
    public async Task UpdateRequestStatusAsync_Valid_PersistsStatus()
    {
        var (service, dispatch, _, _, _) = CreateService();
        dispatch.GetByIdAsync(RequestId).Returns(new ERRequest { Id = RequestId });

        await service.UpdateRequestStatusAsync(RequestId, AssignedStatus);

        await dispatch.Received().UpdateAsync(Arg.Is<ERRequest>(request => request.Status == AssignedStatus));
    }

    [TestMethod]
    public async Task GetPendingRequestIdsAsync_ReturnsOrderedIds()
    {
        var (service, dispatch, _, _, _) = CreateService();
        dispatch.GetPendingAsync().Returns(new List<ERRequest> { new() { Id = RequestId, CreatedAt = DateTime.UtcNow } });

        var result = await service.GetPendingRequestIdsAsync();

        Assert.AreEqual(RequestId, result[0]);
    }

    [TestMethod]
    public async Task DispatchERRequestAsync_MatchingDoctor_ReturnsSuccess()
    {
        var (service, dispatch, staff, shifts, _) = CreateService();
        dispatch.GetByIdAsync(RequestId).Returns(new ERRequest { Id = RequestId, Specialization = Specialization, Location = Location });
        staff.GetAllDoctorsAsync().Returns(new List<Doctor> { new() { StaffId = DoctorId, FirstName = DoctorName, DoctorStatus = DoctorStatus.Available, Specialization = Specialization } });
        shifts.GetCurrentShiftsAsync().Returns(new List<Shift> { new() { Staff = new Staff { StaffId = DoctorId }, Location = Location } });

        var result = await service.DispatchERRequestAsync(RequestId);

        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public async Task ManualOverrideAsync_NotNearShiftEnd_ReturnsFailure()
    {
        var (service, dispatch, staff, shifts, _) = CreateService();
        dispatch.GetByIdAsync(RequestId).Returns(new ERRequest { Id = RequestId });
        staff.GetByIdAsync(DoctorId).Returns(new Doctor { StaffId = DoctorId });
        shifts.GetByStaffIdAsync(DoctorId).Returns(new List<Shift>());

        var result = await service.ManualOverrideAsync(RequestId, DoctorId, NearEndMinutes);

        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public async Task ManualOverrideAsync_NearShiftEnd_ReturnsSuccess()
    {
        var (service, dispatch, staff, shifts, _) = CreateService();
        dispatch.GetByIdAsync(RequestId).Returns(new ERRequest { Id = RequestId });
        staff.GetByIdAsync(DoctorId).Returns(new Doctor { StaffId = DoctorId });
        shifts.GetByStaffIdAsync(DoctorId).Returns(new List<Shift>
        {
            new() { Staff = new Staff { StaffId = DoctorId }, Status = ShiftStatus.Active, EndTime = ShiftEndNearNow },
        });

        var result = await service.ManualOverrideAsync(RequestId, DoctorId, NearEndMinutes);

        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public async Task GetManualOverrideCandidatesAsync_ReturnsNearEndDoctors()
    {
        var (service, _, staff, shifts, _) = CreateService();
        staff.GetAllDoctorsAsync().Returns(new List<Doctor> { new() { StaffId = DoctorId } });
        shifts.GetAllAsync().Returns(new List<Shift>
        {
            new() { Staff = new Staff { StaffId = DoctorId }, Status = ShiftStatus.Active, EndTime = ShiftEndNearNow },
        });

        var result = await service.GetManualOverrideCandidatesAsync(RequestId, NearEndMinutes);

        Assert.HasCount(1, result);
    }
}
