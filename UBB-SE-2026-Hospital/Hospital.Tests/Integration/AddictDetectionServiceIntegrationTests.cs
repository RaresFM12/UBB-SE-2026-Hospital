using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services.PatientEr;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Hospital.Tests.Integration
{
    // Integration tests for AddictDetectionService (service -> repositories -> EF Core).
    // Two tests per public service function.
    // NOTE: GetAddictCandidatesAsync relies on a GroupBy/SelectMany repository query that the
    // EF Core InMemory provider cannot translate, so those two tests mock the prescription
    // repository while the rest remain full integration tests.
    [TestClass]
    public sealed class AddictDetectionServiceIntegrationTests
    {
        private static AddictDetectionService CreateService(Hospital.Data.HospitalDbContext context)
            => new AddictDetectionService(
                new PrescriptionRepository(context),
                new MedicalHistoryRepository(context));

        private static Patient NewPatient()
            => new Patient
            {
                FirstName = "Jane", LastName = "Doe", Cnp = "1234567890123",
                DateOfBirth = new DateTime(1990, 1, 1), Sex = Sex.F,
                PhoneNumber = "0700000000", EmergencyContact = "X",
            };

        // ---- GetAddictCandidatesAsync ----
        [TestMethod]
        public async Task GetAddictCandidatesAsync_WhenNoFlaggedPrescriptions_ReturnsEmpty()
        {
            var prescriptionRepo = new Mock<IPrescriptionRepository>();
            prescriptionRepo.Setup(r => r.GetPotentialDrugAddictsAsync()).ReturnsAsync(new List<Prescription>());
            prescriptionRepo.Setup(r => r.GetPoliceNotifiedPatientIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<int>());
            var historyRepo = new Mock<IMedicalHistoryRepository>();
            var service = new AddictDetectionService(prescriptionRepo.Object, historyRepo.Object);

            List<Patient> result = await service.GetAddictCandidatesAsync();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetAddictCandidatesAsync_WhenFlaggedPrescription_ReturnsDistinctPatient()
        {
            var patient = NewPatient();
            patient.PatientId = 1;
            var history = new MedicalHistory { Patient = patient };
            var record = new MedicalRecord { MedicalHistory = history, SourceType = SourceType.ER, ConsultationDate = DateTime.UtcNow, BasePrice = 0, FinalPrice = 0 };
            var prescription = new Prescription { MedicalRecord = record, Date = DateTime.UtcNow };

            var prescriptionRepo = new Mock<IPrescriptionRepository>();
            prescriptionRepo.Setup(r => r.GetPotentialDrugAddictsAsync())
                .ReturnsAsync(new List<Prescription> { prescription });
            prescriptionRepo.Setup(r => r.GetPoliceNotifiedPatientIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<int>());
            var historyRepo = new Mock<IMedicalHistoryRepository>();
            historyRepo.Setup(r => r.GetByPatientIdAsync(It.IsAny<int>())).ReturnsAsync((MedicalHistory?)null);
            var service = new AddictDetectionService(prescriptionRepo.Object, historyRepo.Object);

            List<Patient> result = await service.GetAddictCandidatesAsync();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].PatientId);
        }

        // ---- MarkPoliceNotifiedAsync ----
        [TestMethod]
        public async Task MarkPoliceNotifiedAsync_WhenInvalidId_Throws()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await CreateService(context).MarkPoliceNotifiedAsync(0));
        }

        [TestMethod]
        public async Task MarkPoliceNotifiedAsync_WhenValid_MarksRecords()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var patient = NewPatient();
            var history = new MedicalHistory { Patient = patient };
            var record = new MedicalRecord { MedicalHistory = history, SourceType = SourceType.ER, ConsultationDate = DateTime.UtcNow, BasePrice = 0, FinalPrice = 0 };
            context.MedicalRecords.Add(record);
            await context.SaveChangesAsync();

            await CreateService(context).MarkPoliceNotifiedAsync(patient.PatientId);

            Assert.IsTrue((await new MedicalRecordRepository(context).GetByIdAsync(record.RecordId))!.PoliceNotified);
        }

        // ---- BuildPoliceReportAsync ----
        [TestMethod]
        public async Task BuildPoliceReportAsync_WhenInvalidId_Throws()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await CreateService(context).BuildPoliceReportAsync(0));
        }

        [TestMethod]
        public async Task BuildPoliceReportAsync_WhenNoPrescriptions_ThrowsPatientNotFound()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await CreateService(context).BuildPoliceReportAsync(123));
        }

        // ---- GetChronicConditionsAsync ----
        [TestMethod]
        public async Task GetChronicConditionsAsync_WhenInvalidId_Throws()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await CreateService(context).GetChronicConditionsAsync(0));
        }

        [TestMethod]
        public async Task GetChronicConditionsAsync_WhenNoHistory_ReturnsNoneReported()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            string result = await CreateService(context).GetChronicConditionsAsync(123);
            Assert.AreEqual("None reported.", result);
        }
    }
}
