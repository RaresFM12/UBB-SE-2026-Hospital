using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services.PatientEr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hospital.Tests.Integration
{
    // Integration tests for AllergyService exercised against the real AllergyRepository
    // and an in-memory HospitalDbContext (full service -> repository -> EF Core stack).
    [TestClass]
    public sealed class AllergyServiceIntegrationTests
    {
        [TestMethod]
        public async Task GetAllergiesAsync_WhenAllergiesPersisted_ReturnsAllFromDatabase()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            context.Allergies.Add(new Allergy { AllergyName = "Penicillin", AllergyType = "Drug" });
            context.Allergies.Add(new Allergy { AllergyName = "Peanuts", AllergyType = "Food" });
            await context.SaveChangesAsync();

            var service = new AllergyService(new AllergyRepository(context));

            List<Allergy> result = await service.GetAllergiesAsync();

            Assert.AreEqual(2, result.Count);
            CollectionAssert.AreEquivalent(
                new[] { "Penicillin", "Peanuts" },
                result.Select(a => a.AllergyName).ToList());
        }

        [TestMethod]
        public async Task GetAllergiesAsync_WhenDatabaseEmpty_ReturnsEmptyList()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var service = new AllergyService(new AllergyRepository(context));

            List<Allergy> result = await service.GetAllergiesAsync();

            Assert.AreEqual(0, result.Count);
        }
    }
}
