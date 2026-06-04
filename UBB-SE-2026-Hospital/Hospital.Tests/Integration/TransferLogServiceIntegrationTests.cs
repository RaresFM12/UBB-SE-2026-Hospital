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
    // Integration tests for TransferLogService exercised against the real repositories
    // and an in-memory HospitalDbContext (full service -> repository -> EF Core stack).
    // Two tests per public service function.
    [TestClass]
    public sealed class TransferLogServiceIntegrationTests
    {
        private static TransferLogService CreateService(Hospital.Data.HospitalDbContext context)
            => new TransferLogService(
                new TransferLogRepository(context),
                new ERVisitRepository(context));

        private static ERVisit NewVisit(string status = ERVisit.VisitStatus.IN_EXAMINATION)
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

        private static async Task<ERVisit> PersistVisitAsync(Hospital.Data.HospitalDbContext context, string status = ERVisit.VisitStatus.IN_EXAMINATION)
        {
            ERVisit visit = NewVisit(status);
            context.ERVisits.Add(visit);
            await context.SaveChangesAsync();
            return visit;
        }

        private static TransferLog NewLog(ERVisit visit, string target = "RegionalHospital", string status = "SUCCESS")
            => new TransferLog
            {
                Visit = visit,
                TargetSystem = target,
                TransferTime = DateTime.Now,
                Status = status,
            };

        // ---- CreateAsync ----
        [TestMethod]
        public async Task CreateAsync_AssignsIdentity()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit visit = await PersistVisitAsync(context);
            var service = CreateService(context);

            TransferLog created = await service.CreateAsync(NewLog(visit));

            Assert.IsTrue(created.TransferLogId > 0);
        }

        [TestMethod]
        public async Task CreateAsync_PersistsRetrievableLog()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit visit = await PersistVisitAsync(context);
            var service = CreateService(context);

            TransferLog created = await service.CreateAsync(NewLog(visit, target: "CityClinic"));
            TransferLog? fetched = await service.GetByIdAsync(created.TransferLogId);

            Assert.IsNotNull(fetched);
            Assert.AreEqual("CityClinic", fetched!.TargetSystem);
        }

        // ---- GetAllAsync ----
        [TestMethod]
        public async Task GetAllAsync_WhenEmpty_ReturnsEmptyList()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var service = CreateService(context);

            List<TransferLog> result = await service.GetAllAsync();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetAllAsync_WhenLogsExist_ReturnsAll()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit visit = await PersistVisitAsync(context);
            var service = CreateService(context);
            await service.CreateAsync(NewLog(visit));
            await service.CreateAsync(NewLog(visit, target: "CityClinic"));

            List<TransferLog> result = await service.GetAllAsync();

            Assert.AreEqual(2, result.Count);
        }

        // ---- GetByIdAsync ----
        [TestMethod]
        public async Task GetByIdAsync_WhenLogExists_ReturnsLog()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit visit = await PersistVisitAsync(context);
            var service = CreateService(context);
            TransferLog created = await service.CreateAsync(NewLog(visit));

            TransferLog? fetched = await service.GetByIdAsync(created.TransferLogId);

            Assert.IsNotNull(fetched);
            Assert.AreEqual(created.TransferLogId, fetched!.TransferLogId);
        }

        [TestMethod]
        public async Task GetByIdAsync_WhenLogMissing_ReturnsNull()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var service = CreateService(context);

            TransferLog? fetched = await service.GetByIdAsync(999);

            Assert.IsNull(fetched);
        }

        // ---- UpdateAsync ----
        [TestMethod]
        public async Task UpdateAsync_PersistsChangedFields()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit visit = await PersistVisitAsync(context);
            var service = CreateService(context);
            TransferLog created = await service.CreateAsync(NewLog(visit, status: "RETRYING"));

            created.Status = "SUCCESS";
            await service.UpdateAsync(created);

            TransferLog? fetched = await service.GetByIdAsync(created.TransferLogId);
            Assert.AreEqual("SUCCESS", fetched!.Status);
        }

        [TestMethod]
        public async Task UpdateAsync_ReturnsSameLogId()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit visit = await PersistVisitAsync(context);
            var service = CreateService(context);
            TransferLog created = await service.CreateAsync(NewLog(visit));

            created.TargetSystem = "UpdatedTarget";
            TransferLog updated = await service.UpdateAsync(created);

            Assert.AreEqual(created.TransferLogId, updated.TransferLogId);
            Assert.AreEqual("UpdatedTarget", updated.TargetSystem);
        }

        // ---- DeleteAsync ----
        [TestMethod]
        public async Task DeleteAsync_WhenLogExists_RemovesLog()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit visit = await PersistVisitAsync(context);
            var service = CreateService(context);
            TransferLog created = await service.CreateAsync(NewLog(visit));

            await service.DeleteAsync(created.TransferLogId);

            Assert.IsNull(await service.GetByIdAsync(created.TransferLogId));
        }

        [TestMethod]
        public async Task DeleteAsync_WhenLogMissing_DoesNotThrow()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var service = CreateService(context);

            await service.DeleteAsync(999);

            Assert.AreEqual(0, (await service.GetAllAsync()).Count);
        }

        // ---- GetByVisitIdAsync ----
        [TestMethod]
        public async Task GetByVisitIdAsync_ReturnsLogsForVisit()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit visit = await PersistVisitAsync(context);
            var service = CreateService(context);
            await service.CreateAsync(NewLog(visit));

            List<TransferLog> logs = await service.GetByVisitIdAsync(visit.VisitId);

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(visit.VisitId, logs[0].Visit.VisitId);
        }

        [TestMethod]
        public async Task GetByVisitIdAsync_WhenNoLogsForVisit_ReturnsEmpty()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            ERVisit visit = await PersistVisitAsync(context);
            var service = CreateService(context);

            List<TransferLog> logs = await service.GetByVisitIdAsync(visit.VisitId);

            Assert.AreEqual(0, logs.Count);
        }

        // ---- GetEligibleVisitsAsync ----
        [TestMethod]
        public async Task GetEligibleVisitsAsync_ReturnsOnlyVisitsInExamination()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            context.ERVisits.Add(NewVisit(ERVisit.VisitStatus.IN_EXAMINATION));
            context.ERVisits.Add(NewVisit(ERVisit.VisitStatus.REGISTERED));
            await context.SaveChangesAsync();
            var service = CreateService(context);

            List<ERTransferEligibleVisit> eligible = await service.GetEligibleVisitsAsync();

            Assert.AreEqual(1, eligible.Count);
            Assert.AreEqual(ERVisit.VisitStatus.IN_EXAMINATION, eligible[0].Status);
        }

        [TestMethod]
        public async Task GetEligibleVisitsAsync_WhenNoneInExamination_ReturnsEmpty()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            context.ERVisits.Add(NewVisit(ERVisit.VisitStatus.REGISTERED));
            await context.SaveChangesAsync();
            var service = CreateService(context);

            List<ERTransferEligibleVisit> eligible = await service.GetEligibleVisitsAsync();

            Assert.AreEqual(0, eligible.Count);
        }
    }
}
