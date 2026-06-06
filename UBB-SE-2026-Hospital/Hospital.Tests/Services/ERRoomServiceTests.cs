using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class ERRoomServiceTests
{
    private const int RoomId = 101;

    private static (ERRoomService Service, IERRoomRepository Rooms, IERVisitRepository Visits, ITriageRepository Triage) CreateService()
    {
        var rooms = Substitute.For<IERRoomRepository>();
        var visits = Substitute.For<IERVisitRepository>();
        var triage = Substitute.For<ITriageRepository>();
        return (new ERRoomService(rooms, visits, triage), rooms, visits, triage);
    }

    [TestMethod]
    public async Task UpdateAsync_RoomNotFound_ThrowsArgumentException()
    {
        var (service, rooms, _, _) = CreateService();
        rooms.GetByIdAsync(RoomId).Returns((ERRoom?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdateAsync(new ERRoom { RoomId = RoomId }));
    }

    [TestMethod]
    public async Task GetVisitDetailsAsync_RoomNotFound_ThrowsArgumentException()
    {
        var (service, rooms, _, _) = CreateService();
        rooms.GetByIdAsync(RoomId).Returns((ERRoom?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.GetVisitDetailsAsync(RoomId));
    }

    [TestMethod]
    public async Task GetVisitDetailsAsync_NoCurrentVisit_ReturnsNull()
    {
        var (service, rooms, _, _) = CreateService();
        rooms.GetByIdAsync(RoomId).Returns(new ERRoom { RoomId = RoomId, CurrentVisit = null });

        var details = await service.GetVisitDetailsAsync(RoomId);

        Assert.IsNull(details);
    }

    [TestMethod]
    public async Task MarkRoomAsAvailableAsync_RoomNotFound_ThrowsArgumentException()
    {
        var (service, rooms, _, _) = CreateService();
        rooms.GetByIdAsync(RoomId).Returns((ERRoom?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.MarkRoomAsAvailableAsync(RoomId));
    }

    [TestMethod]
    public async Task MarkRoomAsCleaningAsync_RoomNotFound_ThrowsArgumentException()
    {
        var (service, rooms, _, _) = CreateService();
        rooms.GetByIdAsync(RoomId).Returns((ERRoom?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.MarkRoomAsCleaningAsync(RoomId));
    }

    [TestMethod]
    public async Task GetByStatusAsync_FiltersByAvailabilityStatus()
    {
        var (service, rooms, _, _) = CreateService();
        rooms.GetAllAsync().Returns(new List<ERRoom>
        {
            new() { RoomId = RoomId, AvailabilityStatus = ERRoom.RoomStatus.Available },
            new() { RoomId = RoomId + 1, AvailabilityStatus = ERRoom.RoomStatus.Occupied },
        });

        var result = await service.GetByStatusAsync(ERRoom.RoomStatus.Available);

        Assert.HasCount(1, result);
    }

    private const int VisitId = 92;

    [TestMethod]
    public async Task GetAllAsync_ReturnsRepositoryResult()
    {
        var (service, rooms, _, _) = CreateService();
        rooms.GetAllAsync().Returns(new List<ERRoom> { new() { RoomId = RoomId } });

        var result = await service.GetAllAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetByIdAsync_ReturnsRepositoryResult()
    {
        var (service, rooms, _, _) = CreateService();
        rooms.GetByIdAsync(RoomId).Returns(new ERRoom { RoomId = RoomId });

        var result = await service.GetByIdAsync(RoomId);

        Assert.AreEqual(RoomId, result!.RoomId);
    }

    [TestMethod]
    public async Task CreateAsync_DelegatesToRepository()
    {
        var (service, rooms, _, _) = CreateService();
        var room = new ERRoom { RoomId = RoomId };
        rooms.CreateAsync(room).Returns(room);

        var result = await service.CreateAsync(room);

        Assert.AreEqual(RoomId, result.RoomId);
    }

    [TestMethod]
    public async Task UpdateAsync_Existing_PersistsChanges()
    {
        var (service, rooms, _, _) = CreateService();
        rooms.GetByIdAsync(RoomId).Returns(new ERRoom { RoomId = RoomId });
        rooms.UpdateAsync(Arg.Any<ERRoom>()).Returns(call => (ERRoom)call[0]);

        var result = await service.UpdateAsync(new ERRoom { RoomId = RoomId, RoomTypeName = ERRoom.RoomType.GeneralRoom });

        Assert.AreEqual(RoomId, result.RoomId);
    }

    [TestMethod]
    public async Task GetVisitDetailsAsync_WithVisit_ReturnsDetails()
    {
        var (service, rooms, visits, triage) = CreateService();
        var visit = new ERVisit { VisitId = VisitId, Patient = new Patient() };
        rooms.GetByIdAsync(RoomId).Returns(new ERRoom { RoomId = RoomId, CurrentVisit = visit });
        visits.GetByIdAsync(VisitId).Returns(visit);
        triage.GetByVisitIdAsync(VisitId).Returns(new Triage { TriageId = 1 });

        var result = await service.GetVisitDetailsAsync(RoomId);

        Assert.AreEqual(VisitId, result!.Visit.VisitId);
    }

    [TestMethod]
    public async Task MarkRoomAsAvailableAsync_Existing_UpdatesRoom()
    {
        var (service, rooms, _, _) = CreateService();
        rooms.GetByIdAsync(RoomId).Returns(new ERRoom { RoomId = RoomId, AvailabilityStatus = ERRoom.RoomStatus.Cleaning });

        await service.MarkRoomAsAvailableAsync(RoomId);

        await rooms.Received().UpdateAsync(Arg.Is<ERRoom>(room => room.AvailabilityStatus == ERRoom.RoomStatus.Available));
    }

    [TestMethod]
    public async Task MarkRoomAsCleaningAsync_Existing_UpdatesRoom()
    {
        var (service, rooms, _, _) = CreateService();
        rooms.GetByIdAsync(RoomId).Returns(new ERRoom { RoomId = RoomId, AvailabilityStatus = ERRoom.RoomStatus.Occupied, CurrentVisit = null });

        await service.MarkRoomAsCleaningAsync(RoomId);

        await rooms.Received().UpdateAsync(Arg.Is<ERRoom>(room => room.AvailabilityStatus == ERRoom.RoomStatus.Cleaning));
    }

    [TestMethod]
    public async Task DeleteAsync_DelegatesToRepository()
    {
        var (service, rooms, _, _) = CreateService();

        await service.DeleteAsync(RoomId);

        await rooms.Received().DeleteAsync(RoomId);
    }

    [TestMethod]
    public async Task MarkRoomAsCleaningAsync_WithActiveVisit_RequeuesVisit()
    {
        var (service, rooms, visits, _) = CreateService();
        var visit = new ERVisit { VisitId = VisitId, Status = ERVisit.VisitStatus.IN_ROOM, Patient = new Patient() };
        rooms.GetByIdAsync(RoomId).Returns(new ERRoom { RoomId = RoomId, AvailabilityStatus = ERRoom.RoomStatus.Occupied, CurrentVisit = visit });
        visits.GetByIdAsync(VisitId).Returns(visit);

        await service.MarkRoomAsCleaningAsync(RoomId);

        await visits.Received().UpdateAsync(Arg.Is<ERVisit>(updated => updated.Status == ERVisit.VisitStatus.WAITING_FOR_ROOM));
    }
}
