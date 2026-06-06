using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class AddictDetectionServiceTests
{
    private const int InvalidPatientId = 0;
    private const int ValidPatientId = 5;
    private const string ExpectedNoConditionsText = "None reported.";

    private static (AddictDetectionService Service, IPrescriptionRepository Prescriptions, IMedicalHistoryRepository Histories) CreateService()
    {
        var prescriptions = Substitute.For<IPrescriptionRepository>();
        var histories = Substitute.For<IMedicalHistoryRepository>();
        return (new AddictDetectionService(prescriptions, histories), prescriptions, histories);
    }

    [TestMethod]
    public async Task MarkPoliceNotifiedAsync_InvalidId_ThrowsArgumentException()
    {
        var (service, _, _) = CreateService();

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.MarkPoliceNotifiedAsync(InvalidPatientId));
    }

    [TestMethod]
    public async Task MarkPoliceNotifiedAsync_ValidId_DelegatesToRepository()
    {
        var (service, prescriptions, _) = CreateService();

        await service.MarkPoliceNotifiedAsync(ValidPatientId);

        await prescriptions.Received().MarkPoliceNotifiedAsync(ValidPatientId);
    }

    [TestMethod]
    public async Task BuildPoliceReportAsync_InvalidId_ThrowsArgumentException()
    {
        var (service, _, _) = CreateService();

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.BuildPoliceReportAsync(InvalidPatientId));
    }

    [TestMethod]
    public async Task GetChronicConditionsAsync_InvalidId_ThrowsArgumentException()
    {
        var (service, _, _) = CreateService();

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.GetChronicConditionsAsync(InvalidPatientId));
    }

    [TestMethod]
    public async Task GetChronicConditionsAsync_NoHistory_ReturnsNoneReported()
    {
        var (service, _, histories) = CreateService();
        histories.GetByPatientIdAsync(ValidPatientId).Returns((MedicalHistory?)null);

        string result = await service.GetChronicConditionsAsync(ValidPatientId);

        Assert.AreEqual(ExpectedNoConditionsText, result);
    }

    [TestMethod]
    public async Task GetChronicConditionsAsync_WithConditions_JoinsThem()
    {
        var (service, _, histories) = CreateService();
        histories.GetByPatientIdAsync(ValidPatientId)
            .Returns(new MedicalHistory { ChronicConditions = ["Asthma", "Diabetes"] });

        string result = await service.GetChronicConditionsAsync(ValidPatientId);

        Assert.AreEqual("Asthma, Diabetes", result);
    }

    private static Prescription FlaggedPrescription()
    {
        var patient = new Patient { PatientId = ValidPatientId };
        var history = new MedicalHistory { Patient = patient };
        var record = new MedicalRecord { MedicalHistory = history };
        return new Prescription { PrescriptionId = 1, MedicalRecord = record, Date = DateTime.Today };
    }

    [TestMethod]
    public async Task GetAddictCandidatesAsync_ReturnsFlaggedPatients()
    {
        var (service, prescriptions, histories) = CreateService();
        prescriptions.GetPotentialDrugAddictsAsync().Returns(new List<Prescription> { FlaggedPrescription() });
        prescriptions.GetPoliceNotifiedPatientIdsAsync(Arg.Any<IEnumerable<int>>()).Returns(new List<int>());
        histories.GetByPatientIdAsync(ValidPatientId).Returns(new MedicalHistory());

        var result = await service.GetAddictCandidatesAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task BuildPoliceReportAsync_ValidPatient_ReturnsReport()
    {
        var (service, prescriptions, _) = CreateService();
        prescriptions.GetFilteredAsync(Arg.Any<PrescriptionFilter>()).Returns(new List<Prescription> { FlaggedPrescription() });

        string report = await service.BuildPoliceReportAsync(ValidPatientId);

        Assert.IsFalse(string.IsNullOrEmpty(report));
    }
}
