using System;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services.PatientEr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hospital.Tests.Integration
{
    // Integration tests for BillingService (service -> repositories -> EF Core).
    // Two tests per public service function.
    [TestClass]
    public sealed class BillingServiceIntegrationTests
    {
        private static BillingService CreateService(Hospital.Data.HospitalDbContext context)
            => new BillingService(
                new MedicalHistoryRepository(context),
                new MedicalRecordRepository(context),
                new PrescriptionRepository(context),
                new TransplantRepository(context));

        private static Patient NewPatient()
            => new Patient
            {
                FirstName = "Jane", LastName = "Doe", Cnp = "1234567890123",
                DateOfBirth = new DateTime(1990, 1, 1), Sex = Sex.F,
                PhoneNumber = "0700000000", EmergencyContact = "John Doe",
            };

        private static async Task<(int patientId, int recordId)> SeedRecordAsync(
            Hospital.Data.HospitalDbContext context, SourceType source = SourceType.ER)
        {
            var patient = NewPatient();
            var history = new MedicalHistory { Patient = patient };
            var record = new MedicalRecord
            {
                MedicalHistory = history,
                SourceType = source,
                ConsultationDate = DateTime.Today,
                BasePrice = 0,
                FinalPrice = 0,
            };
            context.MedicalRecords.Add(record);
            await context.SaveChangesAsync();
            return (patient.PatientId, record.RecordId);
        }

        // ---- ComputeBasePriceAsync ----
        [TestMethod]
        public async Task ComputeBasePriceAsync_ErRecord_ReturnsErBasePrice()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            (int patientId, int recordId) = await SeedRecordAsync(context, SourceType.ER);

            decimal price = await CreateService(context).ComputeBasePriceAsync(patientId, recordId);

            Assert.AreEqual(500m, price);
        }

        [TestMethod]
        public async Task ComputeBasePriceAsync_AppRecord_ReturnsAppointmentBasePrice()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            (int patientId, int recordId) = await SeedRecordAsync(context, SourceType.App);

            decimal price = await CreateService(context).ComputeBasePriceAsync(patientId, recordId);

            Assert.AreEqual(200m, price);
        }

        // ---- ApplyDiscountAsync ----
        [TestMethod]
        public async Task ApplyDiscountAsync_AppliesPercentage()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            decimal result = await CreateService(context).ApplyDiscountAsync(1000m, 10);
            Assert.AreEqual(900m, result);
        }

        [TestMethod]
        public async Task ApplyDiscountAsync_ZeroDiscount_ReturnsSamePrice()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            decimal result = await CreateService(context).ApplyDiscountAsync(1000m, 0);
            Assert.AreEqual(1000m, result);
        }

        // ---- PersistDiscountAsync ----
        [TestMethod]
        public async Task PersistDiscountAsync_PersistsFinalPrice()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            (_, int recordId) = await SeedRecordAsync(context);

            decimal final = await CreateService(context).PersistDiscountAsync(recordId, 1000m, 20);

            Assert.AreEqual(800m, final);
            Assert.AreEqual(800m, (await new MedicalRecordRepository(context).GetByIdAsync(recordId))!.FinalPrice);
        }

        [TestMethod]
        public async Task PersistDiscountAsync_WhenRecordMissing_ThrowsKeyNotFound()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await Assert.ThrowsExceptionAsync<System.Collections.Generic.KeyNotFoundException>(
                async () => await CreateService(context).PersistDiscountAsync(999, 1000m, 10));
        }
    }
}
