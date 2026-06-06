using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class ERVisitServiceTests
{
    private const int VisitId = 92;
    private const int RoomId = 101;
    private const int PatientId = 8;

    private static (ERVisitService Service, IERVisitRepository Visits, IERRoomRepository Rooms, ITriageRepository Triage, ITriageParametersRepository Parameters, ITransferLogRepository TransferLogs, IPatientRepository Patients) CreateService()
    {
        var visits = Substitute.For<IERVisitRepository>();
        var rooms = Substitute.For<IERRoomRepository>();
        var triage = Substitute.For<ITriageRepository>();
        var parameters = Substitute.For<ITriageParametersRepository>();
        var transferLogs = Substitute.For<ITransferLogRepository>();
        var patients = Substitute.For<IPatientRepository>();
        return (new ERVisitService(visits, rooms, triage, parameters, transferLogs, patients), visits, rooms, triage, parameters, transferLogs, patients);
    }

    [TestMethod]
    public async Task CreateAsync_NullVisit_ThrowsArgumentNullException()
    {
        var (service, _, _, _, _, _, _) = CreateService();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => service.CreateAsync(null!));
    }

    [TestMethod]
    public async Task CreateAsync_MissingPatient_ThrowsArgumentException()
    {
        var (service, _, _, _, _, _, _) = CreateService();

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CreateAsync(new ERVisit { Patient = null! }));
    }

    [TestMethod]
    public async Task CreateAsync_PatientNotFound_ThrowsArgumentException()
    {
        var (service, _, _, _, _, _, patients) = CreateService();
        patients.GetByIdAsync(PatientId).Returns((Patient?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CreateAsync(new ERVisit { Patient = new Patient { PatientId = PatientId } }));
    }

    [TestMethod]
    public async Task UpdateAsync_VisitNotFound_ThrowsArgumentException()
    {
        var (service, visits, _, _, _, _, _) = CreateService();
        visits.GetByIdAsync(VisitId).Returns((ERVisit?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdateAsync(new ERVisit { VisitId = VisitId }));
    }

    [TestMethod]
    public async Task GetByStatusAsync_FiltersByStatus()
    {
        var (service, visits, _, _, _, _, _) = CreateService();
        visits.GetAllAsync().Returns(new List<ERVisit>
        {
            new() { VisitId = VisitId, Status = ERVisit.VisitStatus.REGISTERED, Patient = new Patient() },
            new() { VisitId = VisitId + 1, Status = ERVisit.VisitStatus.CLOSED, Patient = new Patient() },
        });

        var result = await service.GetByStatusAsync(ERVisit.VisitStatus.REGISTERED);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task AssignRoomAsync_RoomNotFound_ThrowsInvalidOperationException()
    {
        var (service, _, rooms, _, _, _, _) = CreateService();
        rooms.GetByIdAsync(RoomId).Returns((ERRoom?)null);

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.AssignRoomAsync(VisitId, RoomId));
    }

    [TestMethod]
    public async Task AssignRoomAsync_RoomNotAvailable_ThrowsInvalidOperationException()
    {
        var (service, _, rooms, _, _, _, _) = CreateService();
        rooms.GetByIdAsync(RoomId).Returns(new ERRoom { RoomId = RoomId, AvailabilityStatus = ERRoom.RoomStatus.Occupied });

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.AssignRoomAsync(VisitId, RoomId));
    }

    [TestMethod]
    public async Task AssignRoomAsync_VisitNotFound_ThrowsInvalidOperationException()
    {
        var (service, visits, rooms, _, _, _, _) = CreateService();
        rooms.GetByIdAsync(RoomId).Returns(new ERRoom { RoomId = RoomId, AvailabilityStatus = ERRoom.RoomStatus.Available });
        visits.GetByIdAsync(VisitId).Returns((ERVisit?)null);

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.AssignRoomAsync(VisitId, RoomId));
    }

    [TestMethod]
    public async Task AutoAssignHighestPriorityRoomAsync_NoWaitingVisits_ReturnsFalse()
    {
        var (service, visits, _, _, _, _, _) = CreateService();
        visits.GetAllAsync().Returns(new List<ERVisit>());

        bool assigned = await service.AutoAssignHighestPriorityRoomAsync();

        Assert.IsFalse(assigned);
    }

    private const int TriageId = 14;

    private static ERVisit VisitWithStatus(string status) => new()
    {
        VisitId = VisitId,
        Status = status,
        Patient = new Patient { PatientId = PatientId },
    };

    [TestMethod]
    public async Task GetAllAsync_ReturnsRepositoryResult()
    {
        var (service, visits, _, _, _, _, _) = CreateService();
        visits.GetAllAsync().Returns(new List<ERVisit> { VisitWithStatus(ERVisit.VisitStatus.REGISTERED) });

        var result = await service.GetAllAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetByPatientIdAsync_ReturnsRepositoryResult()
    {
        var (service, visits, _, _, _, _, _) = CreateService();
        visits.GetByPatientIdAsync(PatientId).Returns(new List<ERVisit> { VisitWithStatus(ERVisit.VisitStatus.REGISTERED) });

        var result = await service.GetByPatientIdAsync(PatientId);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task CreateAsync_Valid_CreatesVisit()
    {
        var (service, visits, _, _, _, _, patients) = CreateService();
        patients.GetByIdAsync(PatientId).Returns(new Patient { PatientId = PatientId });
        visits.CreateAsync(Arg.Any<ERVisit>()).Returns(call => (ERVisit)call[0]);

        await service.CreateAsync(new ERVisit { Patient = new Patient { PatientId = PatientId } });

        await visits.Received().CreateAsync(Arg.Any<ERVisit>());
    }

    [TestMethod]
    public async Task UpdateAsync_Valid_PersistsVisit()
    {
        var (service, visits, _, _, _, _, _) = CreateService();
        visits.GetByIdAsync(VisitId).Returns(VisitWithStatus(ERVisit.VisitStatus.REGISTERED));

        await service.UpdateAsync(new ERVisit { VisitId = VisitId, Status = ERVisit.VisitStatus.TRIAGED });

        await visits.Received().UpdateAsync(Arg.Any<ERVisit>());
    }

    [TestMethod]
    public async Task AssignRoomAsync_Valid_MovesVisitIntoRoom()
    {
        var (service, visits, rooms, _, _, _, _) = CreateService();
        rooms.GetByIdAsync(RoomId).Returns(new ERRoom { RoomId = RoomId, AvailabilityStatus = ERRoom.RoomStatus.Available });
        visits.GetByIdAsync(VisitId).Returns(VisitWithStatus(ERVisit.VisitStatus.WAITING_FOR_ROOM));

        await service.AssignRoomAsync(VisitId, RoomId);

        await visits.Received().UpdateAsync(Arg.Is<ERVisit>(visit => visit.Status == ERVisit.VisitStatus.IN_ROOM));
    }

    [TestMethod]
    public async Task CloseVisitAsync_Valid_SetsClosedStatus()
    {
        var (service, visits, rooms, _, _, _, _) = CreateService();
        visits.GetByIdAsync(VisitId).Returns(VisitWithStatus(ERVisit.VisitStatus.IN_EXAMINATION));
        rooms.GetAllAsync().Returns(new List<ERRoom>());

        await service.CloseVisitAsync(VisitId);

        await visits.Received().UpdateAsync(Arg.Is<ERVisit>(visit => visit.Status == ERVisit.VisitStatus.CLOSED));
    }

    [TestMethod]
    public async Task TransferVisitAsync_Valid_MarksPatientTransferred()
    {
        var (service, visits, rooms, _, _, _, patients) = CreateService();
        visits.GetByIdAsync(VisitId).Returns(VisitWithStatus(ERVisit.VisitStatus.IN_EXAMINATION));
        rooms.GetAllAsync().Returns(new List<ERRoom>());

        await service.TransferVisitAsync(VisitId);

        await patients.Received().UpdateAsync(Arg.Is<Patient>(patient => patient.Transferred));
    }

    [TestMethod]
    public async Task AutoAssignHighestPriorityRoomAsync_MatchingRoom_ReturnsTrue()
    {
        var (service, visits, rooms, triage, parameters, _, _) = CreateService();
        var waitingVisit = VisitWithStatus(ERVisit.VisitStatus.WAITING_FOR_ROOM);
        visits.GetAllAsync().Returns(new List<ERVisit> { waitingVisit });
        visits.GetByIdAsync(VisitId).Returns(VisitWithStatus(ERVisit.VisitStatus.WAITING_FOR_ROOM));
        triage.GetByVisitIdAsync(VisitId).Returns(new Triage { TriageId = TriageId });
        parameters.GetByTriageIdAsync(TriageId).Returns(new TriageParameters());
        rooms.GetAvailableRoomsAsync().Returns(new List<ERRoom> { new() { RoomId = RoomId, RoomTypeName = ERRoom.RoomType.GeneralRoom, AvailabilityStatus = ERRoom.RoomStatus.Available } });
        rooms.GetByIdAsync(RoomId).Returns(new ERRoom { RoomId = RoomId, AvailabilityStatus = ERRoom.RoomStatus.Available });

        bool assigned = await service.AutoAssignHighestPriorityRoomAsync();

        Assert.IsTrue(assigned);
    }

    [TestMethod]
    public async Task GetByIdAsync_ReturnsRepositoryResult()
    {
        var (service, visits, _, _, _, _, _) = CreateService();
        visits.GetByIdAsync(VisitId).Returns(VisitWithStatus(ERVisit.VisitStatus.REGISTERED));

        var result = await service.GetByIdAsync(VisitId);

        Assert.AreEqual(VisitId, result!.VisitId);
    }

    [TestMethod]
    public async Task GetActiveVisitsAsync_ReturnsRepositoryResult()
    {
        var (service, visits, _, _, _, _, _) = CreateService();
        visits.GetActiveVisitsAsync().Returns(new List<ERVisit> { VisitWithStatus(ERVisit.VisitStatus.IN_ROOM) });

        var result = await service.GetActiveVisitsAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task DeleteAsync_DelegatesToRepository()
    {
        var (service, visits, _, _, _, _, _) = CreateService();

        await service.DeleteAsync(VisitId);

        await visits.Received().DeleteAsync(VisitId);
    }

    [TestMethod]
    public async Task RetryTransferAsync_Valid_MarksPatientTransferred()
    {
        var (service, visits, rooms, _, _, _, patients) = CreateService();
        visits.GetByIdAsync(VisitId).Returns(VisitWithStatus(ERVisit.VisitStatus.IN_EXAMINATION));
        rooms.GetAllAsync().Returns(new List<ERRoom>());

        await service.RetryTransferAsync(VisitId);

        await patients.Received().UpdateAsync(Arg.Is<Patient>(patient => patient.Transferred));
    }
}
