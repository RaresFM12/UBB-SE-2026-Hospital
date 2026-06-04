using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services.PatientEr;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedPatient = Hospital.Shared.Models.PatientEr.Patient;

namespace Hospital.Tests.Integration
{
    // Integration tests for PatientService (service -> repository -> EF Core).
    // Two tests per public service function.
    [TestClass]
    public sealed class PatientServiceIntegrationTests
    {
        private static PatientService CreateService(Hospital.Data.HospitalDbContext context)
            => new PatientService(new PatientRepository(context));

        private static Patient NewPatient(string first = "Jane", bool archived = false)
            => new Patient
            {
                FirstName = first,
                LastName = "Doe",
                Cnp = "1234567890123",
                DateOfBirth = new DateTime(1990, 1, 1),
                Sex = Sex.F,
                PhoneNumber = "0700000000",
                EmergencyContact = "John Doe",
                IsArchived = archived,
            };

        // ---- GetPatientsAsync ----
        [TestMethod]
        public async Task GetPatientsAsync_WhenEmpty_ReturnsEmpty()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            IReadOnlyList<SharedPatient> result = await CreateService(context).GetPatientsAsync();
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetPatientsAsync_WhenPatientsExist_ReturnsMappedPatients()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            context.Patients.Add(NewPatient("Alice"));
            context.Patients.Add(NewPatient("Bob"));
            await context.SaveChangesAsync();

            IReadOnlyList<SharedPatient> result = await CreateService(context).GetPatientsAsync();

            Assert.AreEqual(2, result.Count);
        }
    }
}
