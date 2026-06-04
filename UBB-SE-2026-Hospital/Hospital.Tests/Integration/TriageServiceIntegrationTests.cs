using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services.PatientEr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hospital.Tests.Integration
{
    // Integration tests for TriageService (service -> repository -> EF Core).
    // Two tests per public service function.
    [TestClass]
    public sealed class TriageServiceIntegrationTests
    {
        private static TriageService CreateService(Hospital.Data.HospitalDbContext context)
            => new TriageService(new TriageRepository(context));

        private static Triage NewTriage(int level = 3)
            => new Triage { TriageLevel = level, Specialization = "Cardiology", NurseId = 7, TriageTime = DateTime.Now };

        // ---- GetAllAsync ----
        [TestMethod]
        public async Task GetAllAsync_WhenEmpty_ReturnsEmpty()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Assert.AreEqual(0, (await CreateService(context).GetAllAsync()).Count);
        }

        [TestMethod]
        public async Task GetAllAsync_WhenTriagesExist_ReturnsAll()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            context.Triages.Add(NewTriage());
            context.Triages.Add(NewTriage());
            await context.SaveChangesAsync();
            Assert.AreEqual(2, (await CreateService(context).GetAllAsync()).Count);
        }

        // ---- GetByIdAsync ----
        [TestMethod]
        public async Task GetByIdAsync_WhenExists_ReturnsTriage()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Triage t = NewTriage();
            context.Triages.Add(t);
            await context.SaveChangesAsync();
            Triage? result = await CreateService(context).GetByIdAsync(t.TriageId);
            Assert.IsNotNull(result);
            Assert.AreEqual(t.TriageId, result!.TriageId);
        }

        [TestMethod]
        public async Task GetByIdAsync_WhenMissing_ReturnsNull()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Assert.IsNull(await CreateService(context).GetByIdAsync(999));
        }

        // ---- CreateAsync ----
        [TestMethod]
        public async Task CreateAsync_AssignsIdentity()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Triage created = await CreateService(context).CreateAsync(NewTriage());
            Assert.IsTrue(created.TriageId > 0);
        }

        [TestMethod]
        public async Task CreateAsync_PersistsRetrievable()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var service = CreateService(context);
            Triage created = await service.CreateAsync(NewTriage(level: 2));
            Assert.AreEqual(2, (await service.GetByIdAsync(created.TriageId))!.TriageLevel);
        }

        // ---- UpdateAsync ----
        [TestMethod]
        public async Task UpdateAsync_WhenExists_UpdatesFields()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Triage t = NewTriage(level: 5);
            context.Triages.Add(t);
            await context.SaveChangesAsync();
            t.TriageLevel = 1;
            Triage updated = await CreateService(context).UpdateAsync(t);
            Assert.AreEqual(1, updated.TriageLevel);
        }

        [TestMethod]
        public async Task UpdateAsync_WhenMissing_ThrowsArgumentException()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await CreateService(context).UpdateAsync(new Triage { TriageId = 999 }));
        }

        // ---- DeleteAsync ----
        [TestMethod]
        public async Task DeleteAsync_WhenExists_Removes()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Triage t = NewTriage();
            context.Triages.Add(t);
            await context.SaveChangesAsync();
            var service = CreateService(context);
            await service.DeleteAsync(t.TriageId);
            Assert.IsNull(await service.GetByIdAsync(t.TriageId));
        }

        [TestMethod]
        public async Task DeleteAsync_WhenMissing_DoesNotThrow()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var service = CreateService(context);
            await service.DeleteAsync(999);
            Assert.AreEqual(0, (await service.GetAllAsync()).Count);
        }
    }
}
