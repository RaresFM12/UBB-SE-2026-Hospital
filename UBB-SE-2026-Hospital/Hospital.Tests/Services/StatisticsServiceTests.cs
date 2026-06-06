using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class StatisticsServiceTests
{
    private const string ActiveKey = "Active";
    private const string ArchivedKey = "Archived";
    private const string AdultGroup = "Adult (18-64)";
    private const int ExpectedActiveCount = 1;
    private static readonly DateTime AdultBirthDate = new(2000, 1, 1);

    private static (StatisticsService Service, IPatientRepository Patients, IMedicalRecordRepository Records, IPrescriptionRepository Prescriptions) CreateService()
    {
        var patients = Substitute.For<IPatientRepository>();
        var records = Substitute.For<IMedicalRecordRepository>();
        var prescriptions = Substitute.For<IPrescriptionRepository>();
        return (new StatisticsService(patients, records, prescriptions), patients, records, prescriptions);
    }

    [TestMethod]
    public async Task GetActiveVsArchivedRatioAsync_CountsActivePatients()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetAllAsync().Returns(new List<Patient>
        {
            new() { IsArchived = false },
            new() { IsArchived = true },
        });

        var result = await service.GetActiveVsArchivedRatioAsync();

        Assert.AreEqual(ExpectedActiveCount, result[ActiveKey]);
    }

    [TestMethod]
    public async Task GetActiveVsArchivedRatioAsync_CountsArchivedPatients()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetAllAsync().Returns(new List<Patient>
        {
            new() { IsArchived = true },
        });

        var result = await service.GetActiveVsArchivedRatioAsync();

        Assert.AreEqual(ExpectedActiveCount, result[ArchivedKey]);
    }

    [TestMethod]
    public async Task GetAgeDistributionAsync_AssignsAdultBucket()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetAllAsync().Returns(new List<Patient> { new() { DateOfBirth = AdultBirthDate } });

        var result = await service.GetAgeDistributionAsync();

        Assert.AreEqual(ExpectedActiveCount, result[AdultGroup]);
    }

    [TestMethod]
    public async Task GetPatientGenderDistributionAsync_GroupsBySex()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetAllAsync().Returns(new List<Patient> { new() { Sex = Sex.F } });

        var result = await service.GetPatientGenderDistributionAsync();

        Assert.AreEqual(ExpectedActiveCount, result[Sex.F.ToString()]);
    }

    private const string Diagnosis = "FLU";
    private const string Medication = "ASPIRIN";

    private static Patient PatientWithHistory(BloodType bloodType, Rh rh) => new()
    {
        MedicalHistory = new MedicalHistory { BloodType = bloodType, Rh = rh },
    };

    [TestMethod]
    public async Task GetPatientsByBloodTypeAsync_GroupsByBloodType()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetAllAsync().Returns(new List<Patient> { PatientWithHistory(BloodType.A, Rh.Positive) });

        var result = await service.GetPatientsByBloodTypeAsync();

        Assert.AreEqual(ExpectedActiveCount, result[BloodType.A.ToString()]);
    }

    [TestMethod]
    public async Task GetPatientsByRhAsync_GroupsByRh()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetAllAsync().Returns(new List<Patient> { PatientWithHistory(BloodType.A, Rh.Positive) });

        var result = await service.GetPatientsByRhAsync();

        Assert.AreEqual(ExpectedActiveCount, result[Rh.Positive.ToString()]);
    }

    [TestMethod]
    public async Task GetConsultationDistributionAsync_GroupsBySourceType()
    {
        var (service, _, records, _) = CreateService();
        records.GetAllAsync().Returns(new List<MedicalRecord> { new() { SourceType = SourceType.ER } });

        var result = await service.GetConsultationDistributionAsync();

        Assert.AreEqual(ExpectedActiveCount, result[SourceType.ER.ToString()]);
    }

    [TestMethod]
    public async Task GetTopDiagnosesAsync_GroupsByDiagnosis()
    {
        var (service, _, records, _) = CreateService();
        records.GetAllAsync().Returns(new List<MedicalRecord> { new() { Diagnosis = Diagnosis } });

        var result = await service.GetTopDiagnosesAsync();

        Assert.AreEqual(ExpectedActiveCount, result[Diagnosis]);
    }

    [TestMethod]
    public async Task GetMostPrescribedMedsAsync_GroupsByMedication()
    {
        var (service, _, _, prescriptions) = CreateService();
        prescriptions.GetAllAsync().Returns(new List<Prescription>
        {
            new() { MedicationList = [new PrescriptionItem { MedicationName = Medication }] },
        });

        var result = await service.GetMostPrescribedMedsAsync();

        Assert.AreEqual(ExpectedActiveCount, result[Medication]);
    }

    [TestMethod]
    public async Task GetPatientsByRhAsync_IgnoresPatientsWithoutHistory()
    {
        var (service, patients, _, _) = CreateService();
        patients.GetAllAsync().Returns(new List<Patient> { new() });

        var result = await service.GetPatientsByRhAsync();

        Assert.IsEmpty(result);
    }
}
