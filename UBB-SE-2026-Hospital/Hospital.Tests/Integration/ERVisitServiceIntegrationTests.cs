using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services.PatientEr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hospital.Tests.Integration
{
    // Integration tests for ERVisitService (service -> repositories -> EF Core).
    // Two tests per public service function.
    [TestClass]
    public sealed class ERVisitServiceIntegrationTests
    {
        private static ERVisitService CreateService(Hospital.Data.HospitalDbContext context)
            => new ERVisitService(
                new ERVisitRepository(context),
                new ERRoomRepository(context),
                new TriageRepository(context),
                new TriageParametersRepository(context),
                new TransferLogRepository(context),
                new PatientRepository(context));

        private static Patient NewPatient(string first = "Jane")
            => new Patient
            {
                FirstName = first, LastName = "Doe", Cnp = "1234567890123",
                DateOfBirth = new DateTime(1990, 1, 1), Sex = Sex.F,
                PhoneNumber = "0700000000", EmergencyContact = "X",
            };

        private static ERVisit NewVisit(Patient patient, string status = ERVisit.VisitStatus.REGISTERED)
            => new ERVisit { Patient = patient, ChiefComplaint = "Pain", ArrivalDateTime = DateTime.Now, Status = status };

        private static async Task<ERVisit> SeedVisitAsync(
            Hospital.Data.HospitalDbContext context, string status = ERVisit.VisitStatus.REGISTERED, string first = "Jane")
        {
            var visit = NewVisit(NewPatient(first), status);
            context.ERVisits.Add(visit);
            await context.SaveChangesAsync();
            return visit;
        }

        // ---- GetAllAsync ----
        [TestMethod]
        public async Task GetAllAsync_WhenEmpty_ReturnsEmpty()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Assert.AreEqual(0, (await CreateService(context).GetAllAsync()).Count);
        }

        [TestMethod]
        public async Task GetAllAsync_WhenVisitsExist_ReturnsAll()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await SeedVisitAsync(context);
            await SeedVisitAsync(context);
            Assert.AreEqual(2, (await CreateService(context).GetAllAsync()).Count);
        }

        // ---- GetByIdAsync ----
        [TestMethod]
        public async Task GetByIdAsync_WhenExists_ReturnsVisit()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit v = await SeedVisitAsync(context);
            Assert.IsNotNull(await CreateService(context).GetByIdAsync(v.VisitId));
        }

        [TestMethod]
        public async Task GetByIdAsync_WhenMissing_ReturnsNull()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Assert.IsNull(await CreateService(context).GetByIdAsync(999));
        }

        // ---- GetByPatientIdAsync ----
        [TestMethod]
        public async Task GetByPatientIdAsync_ReturnsVisitsForPatient()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit v = await SeedVisitAsync(context);
            Assert.AreEqual(1, (await CreateService(context).GetByPatientIdAsync(v.Patient.PatientId)).Count);
        }

        [TestMethod]
        public async Task GetByPatientIdAsync_WhenNone_ReturnsEmpty()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Assert.AreEqual(0, (await CreateService(context).GetByPatientIdAsync(999)).Count);
        }

        // ---- GetActiveVisitsAsync ----
        [TestMethod]
        public async Task GetActiveVisitsAsync_ExcludesClosed()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await SeedVisitAsync(context, ERVisit.VisitStatus.REGISTERED);
            await SeedVisitAsync(context, ERVisit.VisitStatus.CLOSED);
            Assert.AreEqual(1, (await CreateService(context).GetActiveVisitsAsync()).Count);
        }

        [TestMethod]
        public async Task GetActiveVisitsAsync_ExcludesTransferred()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await SeedVisitAsync(context, ERVisit.VisitStatus.TRANSFERRED);
            Assert.AreEqual(0, (await CreateService(context).GetActiveVisitsAsync()).Count);
        }

        // ---- CreateAsync ----
        [TestMethod]
        public async Task CreateAsync_AssignsIdentity()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var patient = NewPatient();
            context.Patients.Add(patient);
            await context.SaveChangesAsync();
            ERVisit created = await CreateService(context).CreateAsync(NewVisit(patient));
            Assert.IsTrue(created.VisitId > 0);
        }

        [TestMethod]
        public async Task CreateAsync_PersistsRetrievable()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var patient = NewPatient();
            context.Patients.Add(patient);
            await context.SaveChangesAsync();
            var service = CreateService(context);
            ERVisit created = await service.CreateAsync(NewVisit(patient));
            Assert.IsNotNull(await service.GetByIdAsync(created.VisitId));
        }

        // ---- UpdateAsync ----
        [TestMethod]
        public async Task UpdateAsync_WhenExists_UpdatesComplaint()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit v = await SeedVisitAsync(context);
            v.ChiefComplaint = "Updated";
            ERVisit updated = await CreateService(context).UpdateAsync(v);
            Assert.AreEqual("Updated", updated.ChiefComplaint);
        }

        [TestMethod]
        public async Task UpdateAsync_WhenMissing_ThrowsArgumentException()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await CreateService(context).UpdateAsync(new ERVisit { VisitId = 999 }));
        }

        // ---- DeleteAsync ----
        [TestMethod]
        public async Task DeleteAsync_WhenExists_Removes()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit v = await SeedVisitAsync(context);
            var service = CreateService(context);
            await service.DeleteAsync(v.VisitId);
            Assert.IsNull(await service.GetByIdAsync(v.VisitId));
        }

        [TestMethod]
        public async Task DeleteAsync_WhenMissing_DoesNotThrow()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var service = CreateService(context);
            await service.DeleteAsync(999);
            Assert.AreEqual(0, (await service.GetAllAsync()).Count);
        }

        // ---- AutoAssignHighestPriorityRoomAsync ----
        [TestMethod]
        public async Task AutoAssignHighestPriorityRoomAsync_WhenNoWaitingVisits_ReturnsFalse()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Assert.IsFalse(await CreateService(context).AutoAssignHighestPriorityRoomAsync());
        }

        [TestMethod]
        public async Task AutoAssignHighestPriorityRoomAsync_WhenWaitingVisitButNoRoom_ReturnsFalse()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit visit = await SeedVisitAsync(context, ERVisit.VisitStatus.WAITING_FOR_ROOM);
            var triage = new Triage { Visit = visit, TriageLevel = 1, Specialization = "General Surgery", NurseId = 1, TriageTime = DateTime.Now };
            context.Triages.Add(triage);
            await context.SaveChangesAsync();

            Assert.IsFalse(await CreateService(context).AutoAssignHighestPriorityRoomAsync());
        }

        // ---- AssignRoomAsync ----
        [TestMethod]
        public async Task AssignRoomAsync_AssignsRoomAndUpdatesStatuses()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit visit = await SeedVisitAsync(context, ERVisit.VisitStatus.WAITING_FOR_ROOM);
            var room = new ERRoom { RoomTypeName = ERRoom.RoomType.GeneralRoom, AvailabilityStatus = ERRoom.RoomStatus.Available };
            context.ERRooms.Add(room);
            await context.SaveChangesAsync();
            var service = CreateService(context);

            await service.AssignRoomAsync(visit.VisitId, room.RoomId);

            Assert.AreEqual(ERVisit.VisitStatus.IN_ROOM, (await service.GetByIdAsync(visit.VisitId))!.Status);
        }

        [TestMethod]
        public async Task AssignRoomAsync_WhenRoomMissing_Throws()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit visit = await SeedVisitAsync(context, ERVisit.VisitStatus.WAITING_FOR_ROOM);
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await CreateService(context).AssignRoomAsync(visit.VisitId, 999));
        }

        // ---- TransferVisitAsync ----
        [TestMethod]
        public async Task TransferVisitAsync_FromInExamination_SetsTransferred()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit visit = await SeedVisitAsync(context, ERVisit.VisitStatus.IN_EXAMINATION);
            var service = CreateService(context);

            await service.TransferVisitAsync(visit.VisitId);

            Assert.AreEqual(ERVisit.VisitStatus.TRANSFERRED, (await service.GetByIdAsync(visit.VisitId))!.Status);
        }

        [TestMethod]
        public async Task TransferVisitAsync_WhenInvalidTransition_Throws()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit visit = await SeedVisitAsync(context, ERVisit.VisitStatus.REGISTERED);
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await CreateService(context).TransferVisitAsync(visit.VisitId));
        }

        // ---- RetryTransferAsync ----
        [TestMethod]
        public async Task RetryTransferAsync_FromInExamination_SetsTransferred()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit visit = await SeedVisitAsync(context, ERVisit.VisitStatus.IN_EXAMINATION);
            var service = CreateService(context);

            await service.RetryTransferAsync(visit.VisitId);

            Assert.AreEqual(ERVisit.VisitStatus.TRANSFERRED, (await service.GetByIdAsync(visit.VisitId))!.Status);
        }

        [TestMethod]
        public async Task RetryTransferAsync_WhenInvalidTransition_Throws()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit visit = await SeedVisitAsync(context, ERVisit.VisitStatus.REGISTERED);
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await CreateService(context).RetryTransferAsync(visit.VisitId));
        }

        // ---- CloseVisitAsync ----
        [TestMethod]
        public async Task CloseVisitAsync_FromTriaged_SetsClosed()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit visit = await SeedVisitAsync(context, ERVisit.VisitStatus.TRIAGED);
            var service = CreateService(context);

            await service.CloseVisitAsync(visit.VisitId);

            Assert.AreEqual(ERVisit.VisitStatus.CLOSED, (await service.GetByIdAsync(visit.VisitId))!.Status);
        }

        [TestMethod]
        public async Task CloseVisitAsync_WhenInvalidTransition_Throws()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit visit = await SeedVisitAsync(context, ERVisit.VisitStatus.REGISTERED);
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await CreateService(context).CloseVisitAsync(visit.VisitId));
        }
    }
}
