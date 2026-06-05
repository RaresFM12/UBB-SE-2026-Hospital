using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services.PatientEr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hospital.Tests.Integration
{
    // Integration tests for StatisticsService (service -> repositories -> EF Core).
    // Two tests per public service function.
    [TestClass]
    public sealed class StatisticsServiceIntegrationTests
    {
        private static StatisticsService CreateService(Hospital.Data.HospitalDbContext context)
            => new StatisticsService(
                new PatientRepository(context),
                new MedicalRecordRepository(context),
                new PrescriptionRepository(context));

        private static Patient NewPatient(
            Sex sex = Sex.F, bool archived = false, int birthYear = 1990,
            BloodType? blood = null, Rh? rh = null)
        {
            var patient = new Patient
            {
                FirstName = "P", LastName = "Q", Cnp = "1234567890123",
                DateOfBirth = new DateTime(birthYear, 1, 1), Sex = sex,
                PhoneNumber = "0700000000", EmergencyContact = "X", IsArchived = archived,
            };
            if (blood is not null || rh is not null)
            {
                patient.MedicalHistory = new MedicalHistory { Patient = patient, BloodType = blood, Rh = rh };
            }
            return patient;
        }

        private static async Task SeedPatientsAsync(Hospital.Data.HospitalDbContext context, params Patient[] patients)
        {
            context.Patients.AddRange(patients);
            await context.SaveChangesAsync();
        }

        // ---- GetPatientsByBloodTypeAsync ----
        [TestMethod]
        public async Task GetPatientsByBloodTypeAsync_GroupsByBloodType()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await SeedPatientsAsync(context,
                NewPatient(blood: BloodType.A, rh: Rh.Positive),
                NewPatient(blood: BloodType.A, rh: Rh.Positive));

            Dictionary<string, int> result = await CreateService(context).GetPatientsByBloodTypeAsync();

            Assert.AreEqual(2, result["A"]);
        }

        [TestMethod]
        public async Task GetPatientsByBloodTypeAsync_WhenNoHistory_ReturnsEmpty()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await SeedPatientsAsync(context, NewPatient());

            Dictionary<string, int> result = await CreateService(context).GetPatientsByBloodTypeAsync();

            Assert.AreEqual(0, result.Count);
        }

        // ---- GetPatientsByRhAsync ----
        [TestMethod]
        public async Task GetPatientsByRhAsync_GroupsByRh()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await SeedPatientsAsync(context,
                NewPatient(blood: BloodType.O, rh: Rh.Negative));

            Dictionary<string, int> result = await CreateService(context).GetPatientsByRhAsync();

            Assert.AreEqual(1, result["Negative"]);
        }

        [TestMethod]
        public async Task GetPatientsByRhAsync_WhenNoHistory_ReturnsEmpty()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await SeedPatientsAsync(context, NewPatient());

            Dictionary<string, int> result = await CreateService(context).GetPatientsByRhAsync();

            Assert.AreEqual(0, result.Count);
        }

        // ---- GetPatientGenderDistributionAsync ----
        [TestMethod]
        public async Task GetPatientGenderDistributionAsync_GroupsBySex()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await SeedPatientsAsync(context, NewPatient(Sex.F), NewPatient(Sex.M), NewPatient(Sex.F));

            Dictionary<string, int> result = await CreateService(context).GetPatientGenderDistributionAsync();

            Assert.AreEqual(2, result["F"]);
            Assert.AreEqual(1, result["M"]);
        }

        [TestMethod]
        public async Task GetPatientGenderDistributionAsync_WhenEmpty_ReturnsEmpty()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Dictionary<string, int> result = await CreateService(context).GetPatientGenderDistributionAsync();
            Assert.AreEqual(0, result.Count);
        }

        // ---- GetConsultationDistributionAsync ----
        [TestMethod]
        public async Task GetConsultationDistributionAsync_GroupsBySourceType()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var p = NewPatient();
            var history = new MedicalHistory { Patient = p };
            context.MedicalRecords.Add(new MedicalRecord { MedicalHistory = history, SourceType = SourceType.ER, ConsultationDate = DateTime.Today, BasePrice = 0, FinalPrice = 0 });
            await context.SaveChangesAsync();

            Dictionary<string, int> result = await CreateService(context).GetConsultationDistributionAsync();

            Assert.AreEqual(1, result["ER"]);
        }

        [TestMethod]
        public async Task GetConsultationDistributionAsync_WhenEmpty_ReturnsEmpty()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Dictionary<string, int> result = await CreateService(context).GetConsultationDistributionAsync();
            Assert.AreEqual(0, result.Count);
        }

        // ---- GetTopDiagnosesAsync ----
        [TestMethod]
        public async Task GetTopDiagnosesAsync_GroupsByDiagnosis()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var history = new MedicalHistory { Patient = NewPatient() };
            context.MedicalRecords.Add(new MedicalRecord { MedicalHistory = history, SourceType = SourceType.ER, Diagnosis = "Flu", ConsultationDate = DateTime.Today, BasePrice = 0, FinalPrice = 0 });
            await context.SaveChangesAsync();

            Dictionary<string, int> result = await CreateService(context).GetTopDiagnosesAsync();

            Assert.AreEqual(1, result["FLU"]);
        }

        [TestMethod]
        public async Task GetTopDiagnosesAsync_IgnoresBlankDiagnoses()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var history = new MedicalHistory { Patient = NewPatient() };
            context.MedicalRecords.Add(new MedicalRecord { MedicalHistory = history, SourceType = SourceType.ER, Diagnosis = "  ", ConsultationDate = DateTime.Today, BasePrice = 0, FinalPrice = 0 });
            await context.SaveChangesAsync();

            Dictionary<string, int> result = await CreateService(context).GetTopDiagnosesAsync();

            Assert.AreEqual(0, result.Count);
        }

        // ---- GetAgeDistributionAsync ----
        [TestMethod]
        public async Task GetAgeDistributionAsync_BucketsAdult()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await SeedPatientsAsync(context, NewPatient(birthYear: 1990));

            Dictionary<string, int> result = await CreateService(context).GetAgeDistributionAsync();

            Assert.AreEqual(1, result["Adult (18-64)"]);
        }

        [TestMethod]
        public async Task GetAgeDistributionAsync_AlwaysReturnsThreeBuckets()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Dictionary<string, int> result = await CreateService(context).GetAgeDistributionAsync();
            Assert.AreEqual(3, result.Count);
        }

        // ---- GetMostPrescribedMedsAsync ----
        [TestMethod]
        public async Task GetMostPrescribedMedsAsync_CountsMedications()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var history = new MedicalHistory { Patient = NewPatient() };
            var record = new MedicalRecord { MedicalHistory = history, SourceType = SourceType.ER, ConsultationDate = DateTime.Today, BasePrice = 0, FinalPrice = 0 };
            var prescription = new Prescription { MedicalRecord = record, Date = DateTime.Today };
            prescription.MedicationList.Add(new PrescriptionItem { MedicationName = "Aspirin", Prescription = prescription });
            context.Prescriptions.Add(prescription);
            await context.SaveChangesAsync();

            Dictionary<string, int> result = await CreateService(context).GetMostPrescribedMedsAsync();

            Assert.AreEqual(1, result["ASPIRIN"]);
        }

        [TestMethod]
        public async Task GetMostPrescribedMedsAsync_WhenEmpty_ReturnsEmpty()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Dictionary<string, int> result = await CreateService(context).GetMostPrescribedMedsAsync();
            Assert.AreEqual(0, result.Count);
        }

        // ---- GetActiveVsArchivedRatioAsync ----
        [TestMethod]
        public async Task GetActiveVsArchivedRatioAsync_CountsActiveAndArchived()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await SeedPatientsAsync(context, NewPatient(archived: false), NewPatient(archived: true));

            Dictionary<string, int> result = await CreateService(context).GetActiveVsArchivedRatioAsync();

            Assert.AreEqual(1, result["Active"]);
            Assert.AreEqual(1, result["Archived"]);
        }

        [TestMethod]
        public async Task GetActiveVsArchivedRatioAsync_WhenEmpty_ReturnsZeros()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Dictionary<string, int> result = await CreateService(context).GetActiveVsArchivedRatioAsync();
            Assert.AreEqual(0, result["Active"]);
            Assert.AreEqual(0, result["Archived"]);
        }
    }
}
