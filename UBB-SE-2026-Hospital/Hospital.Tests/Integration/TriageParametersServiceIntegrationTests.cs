using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services.PatientEr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hospital.Tests.Integration
{
    // Integration tests for TriageParametersService (service -> repository -> EF Core).
    // Two tests per public service function.
    [TestClass]
    public sealed class TriageParametersServiceIntegrationTests
    {
        private static TriageParametersService CreateService(Hospital.Data.HospitalDbContext context)
            => new TriageParametersService(new TriageParametersRepository(context));

        private static Triage NewTriage()
            => new Triage { TriageLevel = 3, Specialization = "Cardiology", NurseId = 1, TriageTime = DateTime.Now };

        private static TriageParameters NewParams(Triage triage, int pain = 2)
            => new TriageParameters
            {
                Triage = triage,
                Consciousness = 1,
                Breathing = 1,
                Bleeding = 1,
                InjuryType = 1,
                PainLevel = pain,
            };

        private static async Task<Triage> PersistTriageAsync(Hospital.Data.HospitalDbContext context)
        {
            Triage t = NewTriage();
            context.Triages.Add(t);
            await context.SaveChangesAsync();
            return t;
        }

        // ---- GetAllAsync ----
        [TestMethod]
        public async Task GetAllAsync_WhenEmpty_ReturnsEmpty()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Assert.AreEqual(0, (await CreateService(context).GetAllAsync()).Count);
        }

        [TestMethod]
        public async Task GetAllAsync_WhenExist_ReturnsAll()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Triage t = await PersistTriageAsync(context);
            context.TriageParameters.Add(NewParams(t));
            await context.SaveChangesAsync();
            Assert.AreEqual(1, (await CreateService(context).GetAllAsync()).Count);
        }

        // ---- GetByIdAsync ----
        [TestMethod]
        public async Task GetByIdAsync_WhenExists_ReturnsParams()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Triage t = await PersistTriageAsync(context);
            TriageParameters p = NewParams(t);
            context.TriageParameters.Add(p);
            await context.SaveChangesAsync();
            Assert.IsNotNull(await CreateService(context).GetByIdAsync(p.TriageParametersId));
        }

        [TestMethod]
        public async Task GetByIdAsync_WhenMissing_ReturnsNull()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Assert.IsNull(await CreateService(context).GetByIdAsync(999));
        }

        // ---- GetByTriageIdAsync ----
        [TestMethod]
        public async Task GetByTriageIdAsync_WhenExists_ReturnsParams()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Triage t = await PersistTriageAsync(context);
            context.TriageParameters.Add(NewParams(t));
            await context.SaveChangesAsync();
            Assert.IsNotNull(await CreateService(context).GetByTriageIdAsync(t.TriageId));
        }

        [TestMethod]
        public async Task GetByTriageIdAsync_WhenMissing_ReturnsNull()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Assert.IsNull(await CreateService(context).GetByTriageIdAsync(999));
        }

        // ---- CreateAsync ----
        [TestMethod]
        public async Task CreateAsync_AssignsIdentity()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Triage t = await PersistTriageAsync(context);
            TriageParameters created = await CreateService(context).CreateAsync(NewParams(t));
            Assert.IsTrue(created.TriageParametersId > 0);
        }

        [TestMethod]
        public async Task CreateAsync_WhenInvalidParameters_Throws()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Triage t = await PersistTriageAsync(context);
            TriageParameters invalid = NewParams(t);
            invalid.PainLevel = 99;
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(
                async () => await CreateService(context).CreateAsync(invalid));
        }

        // ---- UpdateAsync ----
        [TestMethod]
        public async Task UpdateAsync_WhenExists_UpdatesFields()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Triage t = await PersistTriageAsync(context);
            TriageParameters p = NewParams(t, pain: 1);
            context.TriageParameters.Add(p);
            await context.SaveChangesAsync();
            p.PainLevel = 3;
            TriageParameters updated = await CreateService(context).UpdateAsync(p);
            Assert.AreEqual(3, updated.PainLevel);
        }

        [TestMethod]
        public async Task UpdateAsync_WhenMissing_ThrowsArgumentException()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Triage t = await PersistTriageAsync(context);
            TriageParameters missing = NewParams(t);
            missing.TriageParametersId = 999;
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await CreateService(context).UpdateAsync(missing));
        }

        // ---- DeleteAsync ----
        [TestMethod]
        public async Task DeleteAsync_WhenExists_Removes()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Triage t = await PersistTriageAsync(context);
            TriageParameters p = NewParams(t);
            context.TriageParameters.Add(p);
            await context.SaveChangesAsync();
            var service = CreateService(context);
            await service.DeleteAsync(p.TriageParametersId);
            Assert.IsNull(await service.GetByIdAsync(p.TriageParametersId));
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
