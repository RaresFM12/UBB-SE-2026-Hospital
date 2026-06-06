using Hospital.Data.Models;
using Hospital.Services;

namespace Hospital.Tests.Services;

[TestClass]
public class TriageDecisionServiceTests
{
    private const int CriticalLevel = 1;
    private const int HighLevel = 2;
    private const int ElevatedLevel = 3;
    private const int ModerateLevel = 4;
    private const int LowLevel = 5;

    private const int Critical = 3;
    private const int Elevated = 2;
    private const int Normal = 1;

    private const string GeneralSurgery = "General Surgery";
    private const string Orthopedics = "Orthopedics";
    private const string Pulmonology = "Pulmonology";
    private const string Neurology = "Neurology";
    private const string EmergencyMedicine = "Emergency Medicine";

    private static TriageParameters Parameters(int consciousness, int breathing, int bleeding, int injuryType, int painLevel)
        => new()
        {
            Consciousness = consciousness,
            Breathing = breathing,
            Bleeding = bleeding,
            InjuryType = injuryType,
            PainLevel = painLevel,
        };

    [TestMethod]
    public void CalculateTriageLevel_CriticalConsciousness_ReturnsCriticalLevel()
    {
        var service = new TriageDecisionService();

        int level = service.CalculateTriageLevel(Parameters(Critical, Normal, Normal, Normal, Normal));

        Assert.AreEqual(CriticalLevel, level);
    }

    [TestMethod]
    public void CalculateTriageLevel_HighSeverityScore_ReturnsHighLevel()
    {
        var service = new TriageDecisionService();

        int level = service.CalculateTriageLevel(Parameters(Elevated, Elevated, Elevated, Elevated, Elevated));

        Assert.AreEqual(HighLevel, level);
    }

    [TestMethod]
    public void CalculateTriageLevel_ElevatedSeverityScore_ReturnsElevatedLevel()
    {
        var service = new TriageDecisionService();

        int level = service.CalculateTriageLevel(Parameters(Elevated, Elevated, Normal, Normal, Normal));

        Assert.AreEqual(ElevatedLevel, level);
    }

    [TestMethod]
    public void CalculateTriageLevel_ModerateSeverityScore_ReturnsModerateLevel()
    {
        var service = new TriageDecisionService();

        int level = service.CalculateTriageLevel(Parameters(Elevated, Normal, Normal, Normal, Normal));

        Assert.AreEqual(ModerateLevel, level);
    }

    [TestMethod]
    public void CalculateTriageLevel_LowSeverityScore_ReturnsLowLevel()
    {
        var service = new TriageDecisionService();

        int level = service.CalculateTriageLevel(Parameters(Normal, Normal, Normal, Normal, Normal));

        Assert.AreEqual(LowLevel, level);
    }

    [TestMethod]
    public void CalculateTriageLevel_OutOfRangeParameter_ThrowsArgumentOutOfRange()
    {
        var service = new TriageDecisionService();

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => service.CalculateTriageLevel(Parameters(0, Normal, Normal, Normal, Normal)));
    }

    [TestMethod]
    public void DetermineSpecialization_CriticalBleeding_ReturnsGeneralSurgery()
    {
        var service = new TriageDecisionService();

        string specialization = service.DetermineSpecialization(Parameters(Normal, Normal, Critical, Normal, Normal));

        Assert.AreEqual(GeneralSurgery, specialization);
    }

    [TestMethod]
    public void DetermineSpecialization_ElevatedInjury_ReturnsOrthopedics()
    {
        var service = new TriageDecisionService();

        string specialization = service.DetermineSpecialization(Parameters(Normal, Normal, Normal, Elevated, Normal));

        Assert.AreEqual(Orthopedics, specialization);
    }

    [TestMethod]
    public void DetermineSpecialization_ElevatedBreathing_ReturnsPulmonology()
    {
        var service = new TriageDecisionService();

        string specialization = service.DetermineSpecialization(Parameters(Normal, Elevated, Normal, Normal, Normal));

        Assert.AreEqual(Pulmonology, specialization);
    }

    [TestMethod]
    public void DetermineSpecialization_ElevatedConsciousness_ReturnsNeurology()
    {
        var service = new TriageDecisionService();

        string specialization = service.DetermineSpecialization(Parameters(Elevated, Normal, Normal, Normal, Normal));

        Assert.AreEqual(Neurology, specialization);
    }

    [TestMethod]
    public void DetermineSpecialization_AllNormal_ReturnsEmergencyMedicine()
    {
        var service = new TriageDecisionService();

        string specialization = service.DetermineSpecialization(Parameters(Normal, Normal, Normal, Normal, Normal));

        Assert.AreEqual(EmergencyMedicine, specialization);
    }
}
