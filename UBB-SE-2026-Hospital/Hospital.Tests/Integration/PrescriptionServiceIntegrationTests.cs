using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Data.Repositories;
using Hospital.Services.PatientEr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hospital.Tests.Integration
{
    // Integration tests for PrescriptionService (service -> repository -> EF Core).
    // Two tests per public service function.
    [TestClass]
    public sealed class PrescriptionServiceIntegrationTests
    {
        private static PrescriptionService CreateService(Hospital.Data.HospitalDbContext context)
            => new PrescriptionService(new PrescriptionRepository(context));

        private static async Task<Prescription> PersistPrescriptionAsync(
            Hospital.Data.HospitalDbContext context, DateTime date, string doctorNotes = "n")
        {
            var patient = new Patient
            {
                FirstName = "Jane", LastName = "Doe", Cnp = "1234567890123",
                DateOfBirth = new DateTime(1990, 1, 1), Sex = Sex.F,
                PhoneNumber = "0700000000", EmergencyContact = "John Doe",
            };
            var history = new MedicalHistory { Patient = patient };
            var record = new MedicalRecord
            {
                MedicalHistory = history,
                SourceType = SourceType.ER,
                ConsultationDate = date,
                BasePrice = 0,
                FinalPrice = 0,
            };
            var prescription = new Prescription
            {
                MedicalRecord = record,
                Date = date,
                DoctorNotes = doctorNotes,
            };
            context.Prescriptions.Add(prescription);
            await context.SaveChangesAsync();
            return prescription;
        }

        // ---- GetLatestPrescriptionsAsync ----
        [TestMethod]
        public async Task GetLatestPrescriptionsAsync_ReturnsMostRecentFirst()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await PersistPrescriptionAsync(context, new DateTime(2026, 1, 1));
            Prescription newest = await PersistPrescriptionAsync(context, new DateTime(2026, 6, 1));

            List<Prescription> result = await CreateService(context).GetLatestPrescriptionsAsync(10, 1);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(newest.PrescriptionId, result[0].PrescriptionId);
        }

        [TestMethod]
        public async Task GetLatestPrescriptionsAsync_RespectsPaging()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await PersistPrescriptionAsync(context, new DateTime(2026, 1, 1));
            await PersistPrescriptionAsync(context, new DateTime(2026, 2, 1));

            List<Prescription> page1 = await CreateService(context).GetLatestPrescriptionsAsync(1, 1);

            Assert.AreEqual(1, page1.Count);
        }

        // ---- GetPrescriptionDetailsAsync ----
        [TestMethod]
        public async Task GetPrescriptionDetailsAsync_WhenExists_ReturnsPrescription()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Prescription p = await PersistPrescriptionAsync(context, new DateTime(2026, 3, 1));

            Prescription result = await CreateService(context).GetPrescriptionDetailsAsync(p.PrescriptionId);

            Assert.AreEqual(p.PrescriptionId, result.PrescriptionId);
        }

        [TestMethod]
        public async Task GetPrescriptionDetailsAsync_WhenMissing_ThrowsArgumentException()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await CreateService(context).GetPrescriptionDetailsAsync(999));
        }

        // ---- ApplyFilterAsync ----
        [TestMethod]
        public async Task ApplyFilterAsync_WithDateFilter_ReturnsMatching()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await PersistPrescriptionAsync(context, new DateTime(2026, 1, 1));
            await PersistPrescriptionAsync(context, new DateTime(2026, 6, 1));

            var filter = new PrescriptionFilter { DateFrom = new DateTime(2026, 5, 1) };
            List<Prescription> result = await CreateService(context).ApplyFilterAsync(filter);

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task ApplyFilterAsync_WhenFilterNull_ReturnsTopN()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await PersistPrescriptionAsync(context, new DateTime(2026, 1, 1));

            List<Prescription> result = await CreateService(context).ApplyFilterAsync(null!);

            Assert.AreEqual(1, result.Count);
        }
    }
}
