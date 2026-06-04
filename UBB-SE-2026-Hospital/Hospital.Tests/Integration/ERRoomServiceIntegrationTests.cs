using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Data.Repositories;
using Hospital.Services.PatientEr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hospital.Tests.Integration
{
    // Integration tests for ERRoomService exercised against the real repositories
    // and an in-memory HospitalDbContext (full service -> repository -> EF Core stack).
    // Two tests per public service function.
    [TestClass]
    public sealed class ERRoomServiceIntegrationTests
    {
        private static ERRoomService CreateService(Hospital.Data.HospitalDbContext context)
            => new ERRoomService(
                new ERRoomRepository(context),
                new ERVisitRepository(context),
                new TriageRepository(context));

        private static ERRoom NewRoom(string status = ERRoom.RoomStatus.Available, string type = ERRoom.RoomType.GeneralRoom)
            => new ERRoom { RoomTypeName = type, AvailabilityStatus = status };

        private static ERVisit NewVisit(string status = ERVisit.VisitStatus.IN_ROOM)
            => new ERVisit
            {
                Patient = new Patient
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    Cnp = "1234567890123",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Sex = Sex.F,
                    PhoneNumber = "0700000000",
                    EmergencyContact = "John Doe",
                },
                ChiefComplaint = "Chest pain",
                ArrivalDateTime = DateTime.Now,
                Status = status,
            };

        // ---- GetAllAsync ----
        [TestMethod]
        public async Task GetAllAsync_WhenEmpty_ReturnsEmptyList()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var service = CreateService(context);

            List<ERRoom> result = await service.GetAllAsync();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetAllAsync_WhenRoomsExist_ReturnsAll()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            context.ERRooms.Add(NewRoom());
            context.ERRooms.Add(NewRoom());
            await context.SaveChangesAsync();
            var service = CreateService(context);

            List<ERRoom> result = await service.GetAllAsync();

            Assert.AreEqual(2, result.Count);
        }

        // ---- GetByIdAsync ----
        [TestMethod]
        public async Task GetByIdAsync_WhenRoomExists_ReturnsRoom()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERRoom room = NewRoom();
            context.ERRooms.Add(room);
            await context.SaveChangesAsync();
            var service = CreateService(context);

            ERRoom? result = await service.GetByIdAsync(room.RoomId);

            Assert.IsNotNull(result);
            Assert.AreEqual(room.RoomId, result!.RoomId);
        }

        [TestMethod]
        public async Task GetByIdAsync_WhenRoomMissing_ReturnsNull()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var service = CreateService(context);

            ERRoom? result = await service.GetByIdAsync(999);

            Assert.IsNull(result);
        }

        // ---- CreateAsync ----
        [TestMethod]
        public async Task CreateAsync_AssignsIdentity()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var service = CreateService(context);

            ERRoom created = await service.CreateAsync(NewRoom());

            Assert.IsTrue(created.RoomId > 0);
        }

        [TestMethod]
        public async Task CreateAsync_PersistsRoomRetrievableById()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var service = CreateService(context);

            ERRoom created = await service.CreateAsync(NewRoom(type: ERRoom.RoomType.TraumaBay));
            ERRoom? fetched = await service.GetByIdAsync(created.RoomId);

            Assert.IsNotNull(fetched);
            Assert.AreEqual(ERRoom.RoomType.TraumaBay, fetched!.RoomTypeName);
        }

        // ---- UpdateAsync ----
        [TestMethod]
        public async Task UpdateAsync_WhenRoomExists_UpdatesFields()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERRoom room = NewRoom(type: ERRoom.RoomType.GeneralRoom);
            context.ERRooms.Add(room);
            await context.SaveChangesAsync();
            var service = CreateService(context);

            room.RoomTypeName = ERRoom.RoomType.OperatingRoom;
            ERRoom updated = await service.UpdateAsync(room);

            Assert.AreEqual(ERRoom.RoomType.OperatingRoom, updated.RoomTypeName);
        }

        [TestMethod]
        public async Task UpdateAsync_WhenRoomMissing_ThrowsArgumentException()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var service = CreateService(context);

            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await service.UpdateAsync(new ERRoom { RoomId = 999 }));
        }

        // ---- DeleteAsync ----
        [TestMethod]
        public async Task DeleteAsync_WhenRoomExists_RemovesRoom()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERRoom room = NewRoom();
            context.ERRooms.Add(room);
            await context.SaveChangesAsync();
            var service = CreateService(context);

            await service.DeleteAsync(room.RoomId);

            Assert.IsNull(await service.GetByIdAsync(room.RoomId));
        }

        [TestMethod]
        public async Task DeleteAsync_WhenRoomMissing_DoesNotThrow()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var service = CreateService(context);

            await service.DeleteAsync(999);

            Assert.AreEqual(0, (await service.GetAllAsync()).Count);
        }

        // ---- GetByStatusAsync ----
        [TestMethod]
        public async Task GetByStatusAsync_ReturnsOnlyMatchingRooms()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            context.ERRooms.Add(NewRoom(ERRoom.RoomStatus.Available));
            context.ERRooms.Add(NewRoom(ERRoom.RoomStatus.Occupied));
            context.ERRooms.Add(NewRoom(ERRoom.RoomStatus.Available));
            await context.SaveChangesAsync();
            var service = CreateService(context);

            List<ERRoom> available = await service.GetByStatusAsync(ERRoom.RoomStatus.Available);

            Assert.AreEqual(2, available.Count);
            Assert.IsTrue(available.All(r => r.AvailabilityStatus == ERRoom.RoomStatus.Available));
        }

        [TestMethod]
        public async Task GetByStatusAsync_WhenNoneMatch_ReturnsEmpty()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            context.ERRooms.Add(NewRoom(ERRoom.RoomStatus.Available));
            await context.SaveChangesAsync();
            var service = CreateService(context);

            List<ERRoom> cleaning = await service.GetByStatusAsync(ERRoom.RoomStatus.Cleaning);

            Assert.AreEqual(0, cleaning.Count);
        }

        // ---- GetVisitDetailsAsync ----
        [TestMethod]
        public async Task GetVisitDetailsAsync_WhenRoomHasCurrentVisit_ReturnsDetails()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit visit = NewVisit();
            ERRoom room = NewRoom(ERRoom.RoomStatus.Occupied);
            room.CurrentVisit = visit;
            context.ERVisits.Add(visit);
            context.ERRooms.Add(room);
            await context.SaveChangesAsync();
            var service = CreateService(context);

            ERRoomVisitDetails? details = await service.GetVisitDetailsAsync(room.RoomId);

            Assert.IsNotNull(details);
            Assert.AreEqual(visit.VisitId, details!.Visit!.VisitId);
        }

        [TestMethod]
        public async Task GetVisitDetailsAsync_WhenRoomHasNoVisit_ReturnsNull()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERRoom room = NewRoom();
            context.ERRooms.Add(room);
            await context.SaveChangesAsync();
            var service = CreateService(context);

            ERRoomVisitDetails? details = await service.GetVisitDetailsAsync(room.RoomId);

            Assert.IsNull(details);
        }

        // ---- MarkRoomAsCleaningAsync ----
        [TestMethod]
        public async Task MarkRoomAsCleaningAsync_SetsCleaningAndReleasesVisit()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit visit = NewVisit(ERVisit.VisitStatus.IN_ROOM);
            ERRoom room = NewRoom(ERRoom.RoomStatus.Occupied);
            room.CurrentVisit = visit;
            context.ERVisits.Add(visit);
            context.ERRooms.Add(room);
            await context.SaveChangesAsync();
            var service = CreateService(context);

            await service.MarkRoomAsCleaningAsync(room.RoomId);

            ERRoom? updated = await service.GetByIdAsync(room.RoomId);
            Assert.AreEqual(ERRoom.RoomStatus.Cleaning, updated!.AvailabilityStatus);
            Assert.AreEqual(ERVisit.VisitStatus.WAITING_FOR_ROOM,
                (await new ERVisitRepository(context).GetByIdAsync(visit.VisitId))!.Status);
        }

        [TestMethod]
        public async Task MarkRoomAsCleaningAsync_WhenRoomMissing_ThrowsArgumentException()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var service = CreateService(context);

            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await service.MarkRoomAsCleaningAsync(999));
        }

        // ---- MarkRoomAsAvailableAsync ----
        [TestMethod]
        public async Task MarkRoomAsAvailableAsync_SetsAvailable()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERRoom room = NewRoom(ERRoom.RoomStatus.Cleaning);
            context.ERRooms.Add(room);
            await context.SaveChangesAsync();
            var service = CreateService(context);

            await service.MarkRoomAsAvailableAsync(room.RoomId);

            ERRoom? updated = await service.GetByIdAsync(room.RoomId);
            Assert.AreEqual(ERRoom.RoomStatus.Available, updated!.AvailabilityStatus);
        }

        [TestMethod]
        public async Task MarkRoomAsAvailableAsync_WhenRoomMissing_ThrowsArgumentException()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var service = CreateService(context);

            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await service.MarkRoomAsAvailableAsync(999));
        }
    }
}
