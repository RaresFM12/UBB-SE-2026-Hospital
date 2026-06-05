using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services.PatientEr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hospital.Tests.Integration
{
    // Integration tests for BloodCompatibilityService (service -> repositories -> EF Core).
    // Two tests per public service function.
    [TestClass]
    public sealed class BloodCompatibilityServiceIntegrationTests
    {
        private static BloodCompatibilityService CreateService(Hospital.Data.HospitalDbContext context)
            => new BloodCompatibilityService(
                new PatientRepository(context),
                new MedicalHistoryRepository(context));

        private static Patient NewPatient(string first, Sex sex = Sex.F, int birthYear = 1990)
            => new Patient
            {
                FirstName = first, LastName = "Doe", Cnp = "1234567890123",
                DateOfBirth = new DateTime(birthYear, 1, 1), Sex = sex,
                PhoneNumber = "0700000000", EmergencyContact = "X",
            };

        private static async Task<Patient> SeedPatientWithHistoryAsync(
            Hospital.Data.HospitalDbContext context, string first, BloodType blood, Rh rh, Sex sex = Sex.F)
        {
            var patient = NewPatient(first, sex);
            var history = new MedicalHistory { Patient = patient, BloodType = blood, Rh = rh };
            context.Patients.Add(patient);
            context.MedicalHistories.Add(history);
            await context.SaveChangesAsync();
            return patient;
        }

        // ---- GetTopCompatibleDonorsAsync ----
        [TestMethod]
        public async Task GetTopCompatibleDonorsAsync_ReturnsCompatibleDonor()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Patient recipient = await SeedPatientWithHistoryAsync(context, "Recipient", BloodType.A, Rh.Positive);
            await SeedPatientWithHistoryAsync(context, "Donor", BloodType.O, Rh.Positive);

            List<Patient> donors = await CreateService(context).GetTopCompatibleDonorsAsync(recipient.PatientId);

            Assert.AreEqual(1, donors.Count);
            Assert.AreEqual("Donor", donors[0].FirstName);
        }

        [TestMethod]
        public async Task GetTopCompatibleDonorsAsync_WhenRecipientHasNoBloodType_ReturnsEmpty()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var recipient = NewPatient("Recipient");
            context.Patients.Add(recipient);
            await context.SaveChangesAsync();

            List<Patient> donors = await CreateService(context).GetTopCompatibleDonorsAsync(recipient.PatientId);

            Assert.AreEqual(0, donors.Count);
        }

        // ---- CalculateScore ----
        [TestMethod]
        public void CalculateScore_IdenticalProfile_ScoresHigh()
        {
            var service = new BloodCompatibilityService(null!, null!);
            var donor = NewPatient("D", Sex.F, 1990);
            donor.MedicalHistory = new MedicalHistory { Patient = donor, BloodType = BloodType.A, Rh = Rh.Positive };
            var recipient = NewPatient("R", Sex.F, 1990);
            recipient.MedicalHistory = new MedicalHistory { Patient = recipient, BloodType = BloodType.A, Rh = Rh.Positive };

            int score = service.CalculateScore(donor, recipient);

            Assert.AreEqual(100, score);
        }

        [TestMethod]
        public void CalculateScore_WhenHistoryMissing_ReturnsZero()
        {
            var service = new BloodCompatibilityService(null!, null!);
            int score = service.CalculateScore(NewPatient("D"), NewPatient("R"));
            Assert.AreEqual(0, score);
        }

        // ---- IsBloodMatch ----
        [TestMethod]
        public void IsBloodMatch_ODonor_MatchesAnyReceiver()
        {
            var service = new BloodCompatibilityService(null!, null!);
            Assert.IsTrue(service.IsBloodMatch(BloodType.O, BloodType.AB));
        }

        [TestMethod]
        public void IsBloodMatch_ADonor_DoesNotMatchBReceiver()
        {
            var service = new BloodCompatibilityService(null!, null!);
            Assert.IsFalse(service.IsBloodMatch(BloodType.A, BloodType.B));
        }

        // ---- IsRhMatch ----
        [TestMethod]
        public void IsRhMatch_NegativeDonor_MatchesNegativeReceiver()
        {
            var service = new BloodCompatibilityService(null!, null!);
            Assert.IsTrue(service.IsRhMatch(Rh.Negative, Rh.Negative));
        }

        [TestMethod]
        public void IsRhMatch_PositiveDonor_DoesNotMatchNegativeReceiver()
        {
            var service = new BloodCompatibilityService(null!, null!);
            Assert.IsFalse(service.IsRhMatch(Rh.Positive, Rh.Negative));
        }
    }
}
