using Hospital.Data.Models;
using Hospital.Services.PatientEr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hospital.Tests.Integration
{
    // Tests for TriageDecisionService (pure decision logic, no persistence).
    // Two tests per public service function.
    [TestClass]
    public sealed class TriageDecisionServiceIntegrationTests
    {
        private static TriageParameters Params(int consciousness, int breathing, int bleeding, int injury, int pain)
            => new TriageParameters
            {
                Consciousness = consciousness,
                Breathing = breathing,
                Bleeding = bleeding,
                InjuryType = injury,
                PainLevel = pain,
            };

        // ---- CalculateTriageLevel ----
        [TestMethod]
        public void CalculateTriageLevel_WhenCriticalParameter_ReturnsLevel1()
        {
            var service = new TriageDecisionService();

            int level = service.CalculateTriageLevel(Params(3, 1, 1, 1, 1));

            Assert.AreEqual(1, level);
        }

        [TestMethod]
        public void CalculateTriageLevel_WhenAllMinimal_ReturnsLevel5()
        {
            var service = new TriageDecisionService();

            int level = service.CalculateTriageLevel(Params(1, 1, 1, 1, 1));

            Assert.AreEqual(5, level);
        }

        // ---- DetermineSpecialization ----
        [TestMethod]
        public void DetermineSpecialization_WhenSevereBleeding_ReturnsGeneralSurgery()
        {
            var service = new TriageDecisionService();

            string specialization = service.DetermineSpecialization(Params(1, 1, 3, 1, 1));

            Assert.AreEqual("General Surgery", specialization);
        }

        [TestMethod]
        public void DetermineSpecialization_WhenNothingSpecial_ReturnsEmergencyMedicine()
        {
            var service = new TriageDecisionService();

            string specialization = service.DetermineSpecialization(Params(1, 1, 1, 1, 1));

            Assert.AreEqual("Emergency Medicine", specialization);
        }
    }
}
