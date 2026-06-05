using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services.PatientEr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hospital.Tests.Integration
{
    // Integration tests for TransplantService (service -> repositories + BloodCompatibilityService -> EF Core).
    // Two tests per public service function.
    [TestClass]
    public sealed class TransplantServiceIntegrationTests
    {
        private static TransplantService CreateService(Hospital.Data.HospitalDbContext context)
        {
            var historyRepo = new MedicalHistoryRepository(context);
            var patientRepo = new PatientRepository(context);
            var bloodService = new BloodCompatibilityService(patientRepo, historyRepo);
            return new TransplantService(
                new TransplantRepository(context),
                patientRepo,
                new MedicalRecordRepository(context),
                bloodService,
                historyRepo);
        }

        private static Patient NewPatient(string first = "Jane")
            => new Patient
            {
                FirstName = first, LastName = "Doe", Cnp = "1234567890123",
                DateOfBirth = new DateTime(1990, 1, 1), Sex = Sex.F,
                PhoneNumber = "0700000000", EmergencyContact = "X",
            };

        private static Transplant NewTransplant(Patient receiver, string organ = "Kidney")
            => new Transplant
            {
                Receiver = receiver, OrganType = organ, RequestDate = DateTime.UtcNow,
                Status = TransplantStatus.Pending, CompatibilityScore = 0,
            };

        private static async Task<Patient> SeedPatientAsync(Hospital.Data.HospitalDbContext context, string first = "Jane")
        {
            var p = NewPatient(first);
            context.Patients.Add(p);
            await context.SaveChangesAsync();
            return p;
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
            Patient r = await SeedPatientAsync(context);
            context.Transplants.Add(NewTransplant(r));
            await context.SaveChangesAsync();
            Assert.AreEqual(1, (await CreateService(context).GetAllAsync()).Count);
        }

        // ---- GetByIdAsync ----
        [TestMethod]
        public async Task GetByIdAsync_WhenExists_ReturnsTransplant()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Patient r = await SeedPatientAsync(context);
            var t = NewTransplant(r);
            context.Transplants.Add(t);
            await context.SaveChangesAsync();
            Assert.IsNotNull(await CreateService(context).GetByIdAsync(t.TransplantId));
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
            Patient r = await SeedPatientAsync(context);
            Transplant created = await CreateService(context).CreateAsync(NewTransplant(r));
            Assert.IsTrue(created.TransplantId > 0);
        }

        [TestMethod]
        public async Task CreateAsync_PersistsRetrievable()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Patient r = await SeedPatientAsync(context);
            var service = CreateService(context);
            Transplant created = await service.CreateAsync(NewTransplant(r, "Liver"));
            Assert.AreEqual("Liver", (await service.GetByIdAsync(created.TransplantId))!.OrganType);
        }

        // ---- UpdateAsync ----
        [TestMethod]
        public async Task UpdateAsync_PersistsChanges()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Patient r = await SeedPatientAsync(context);
            var t = NewTransplant(r);
            context.Transplants.Add(t);
            await context.SaveChangesAsync();
            t.Status = TransplantStatus.Completed;
            Transplant updated = await CreateService(context).UpdateAsync(t);
            Assert.AreEqual(TransplantStatus.Completed, updated.Status);
        }

        [TestMethod]
        public async Task UpdateAsync_ReturnsSameId()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Patient r = await SeedPatientAsync(context);
            var t = NewTransplant(r);
            context.Transplants.Add(t);
            await context.SaveChangesAsync();
            Transplant updated = await CreateService(context).UpdateAsync(t);
            Assert.AreEqual(t.TransplantId, updated.TransplantId);
        }

        // ---- DeleteAsync ----
        [TestMethod]
        public async Task DeleteAsync_WhenExists_Removes()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Patient r = await SeedPatientAsync(context);
            var t = NewTransplant(r);
            context.Transplants.Add(t);
            await context.SaveChangesAsync();
            var service = CreateService(context);
            await service.DeleteAsync(t.TransplantId);
            Assert.IsNull(await service.GetByIdAsync(t.TransplantId));
        }

        [TestMethod]
        public async Task DeleteAsync_WhenMissing_DoesNotThrow()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var service = CreateService(context);
            await service.DeleteAsync(999);
            Assert.AreEqual(0, (await service.GetAllAsync()).Count);
        }

        // ---- GetByPatientIdAsync ----
        [TestMethod]
        public async Task GetByPatientIdAsync_ReturnsTransplantsForReceiver()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Patient r = await SeedPatientAsync(context);
            context.Transplants.Add(NewTransplant(r));
            await context.SaveChangesAsync();
            Assert.AreEqual(1, (await CreateService(context).GetByPatientIdAsync(r.PatientId)).Count);
        }

        [TestMethod]
        public async Task GetByPatientIdAsync_WhenNone_ReturnsEmpty()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Assert.AreEqual(0, (await CreateService(context).GetByPatientIdAsync(999)).Count);
        }

        // ---- CreateWaitlistRequestAsync ----
        [TestMethod]
        public async Task CreateWaitlistRequestAsync_CreatesPendingTransplant()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Patient r = await SeedPatientAsync(context);
            var service = CreateService(context);

            await service.CreateWaitlistRequestAsync(r.PatientId, "Kidney");

            List<Transplant> all = await service.GetAllAsync();
            Assert.AreEqual(1, all.Count);
            Assert.AreEqual(TransplantStatus.Pending, all[0].Status);
        }

        [TestMethod]
        public async Task CreateWaitlistRequestAsync_WhenReceiverMissing_Throws()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await CreateService(context).CreateWaitlistRequestAsync(999, "Kidney"));
        }

        // ---- AssignDonorAsync ----
        [TestMethod]
        public async Task AssignDonorAsync_SetsDonorAndCompletes()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Patient receiver = await SeedPatientAsync(context, "Recv");
            Patient donor = await SeedPatientAsync(context, "Donor");
            var t = NewTransplant(receiver);
            context.Transplants.Add(t);
            await context.SaveChangesAsync();
            var service = CreateService(context);

            await service.AssignDonorAsync(t.TransplantId, donor.PatientId, 88f);

            Transplant updated = (await service.GetByIdAsync(t.TransplantId))!;
            Assert.AreEqual(TransplantStatus.Completed, updated.Status);
            Assert.AreEqual(88f, updated.CompatibilityScore);
        }

        [TestMethod]
        public async Task AssignDonorAsync_WhenTransplantMissing_Throws()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Patient donor = await SeedPatientAsync(context, "Donor");
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await CreateService(context).AssignDonorAsync(999, donor.PatientId, 50f));
        }

        // ---- GetTopMatchesAsDisplayModelsAsync ----
        [TestMethod]
        public async Task GetTopMatchesAsDisplayModelsAsync_WhenDonorMissing_Throws()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await CreateService(context).GetTopMatchesAsDisplayModelsAsync(999, "Kidney"));
        }

        [TestMethod]
        public async Task GetTopMatchesAsDisplayModelsAsync_WhenDonorNotDeceased_Throws()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Patient donor = await SeedPatientAsync(context, "Donor");
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await CreateService(context).GetTopMatchesAsDisplayModelsAsync(donor.PatientId, "Kidney"));
        }

        // ---- IsUrgentAsync ----
        [TestMethod]
        public async Task IsUrgentAsync_WhenNoHistory_ReturnsFalse()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Assert.IsFalse(await CreateService(context).IsUrgentAsync(999));
        }

        [TestMethod]
        public async Task IsUrgentAsync_WhenFewErVisits_ReturnsFalse()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Patient p = NewPatient();
            var history = new MedicalHistory { Patient = p };
            context.MedicalHistories.Add(history);
            await context.SaveChangesAsync();
            Assert.IsFalse(await CreateService(context).IsUrgentAsync(p.PatientId));
        }

        // ---- GetChronicWarningAsync ----
        [TestMethod]
        public async Task GetChronicWarningAsync_WhenChronicConditions_ReturnsWarning()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Patient p = NewPatient();
            var history = new MedicalHistory { Patient = p, ChronicConditions = new List<string> { "Diabetes" } };
            context.MedicalHistories.Add(history);
            await context.SaveChangesAsync();

            string? warning = await CreateService(context).GetChronicWarningAsync(p.PatientId);

            Assert.IsNotNull(warning);
        }

        [TestMethod]
        public async Task GetChronicWarningAsync_WhenNoHistory_ReturnsNull()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Assert.IsNull(await CreateService(context).GetChronicWarningAsync(999));
        }
    }
}
