using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class TransplantServiceTests
{
    private const int TransplantId = 41;
    private const int DonorId = 7;
    private const int ReceiverId = 9;
    private const float FinalScore = 80f;
    private const string OrganType = "Kidney";

    private static (TransplantService Service, ITransplantRepository Transplants, IPatientRepository Patients, IMedicalRecordRepository Records, IBloodCompatibilityService Blood, IMedicalHistoryRepository Histories) CreateService()
    {
        var transplants = Substitute.For<ITransplantRepository>();
        var patients = Substitute.For<IPatientRepository>();
        var records = Substitute.For<IMedicalRecordRepository>();
        var blood = Substitute.For<IBloodCompatibilityService>();
        var histories = Substitute.For<IMedicalHistoryRepository>();
        return (new TransplantService(transplants, patients, records, blood, histories), transplants, patients, records, blood, histories);
    }

    [TestMethod]
    public async Task CreateWaitlistRequestAsync_ReceiverNotFound_ThrowsArgumentException()
    {
        var (service, _, patients, _, _, _) = CreateService();
        patients.GetByIdAsync(ReceiverId).Returns((Patient?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CreateWaitlistRequestAsync(ReceiverId, OrganType));
    }

    [TestMethod]
    public async Task AssignDonorAsync_TransplantNotFound_ThrowsArgumentException()
    {
        var (service, transplants, _, _, _, _) = CreateService();
        transplants.GetByIdAsync(TransplantId).Returns((Transplant?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.AssignDonorAsync(TransplantId, DonorId, FinalScore));
    }

    [TestMethod]
    public async Task AssignDonorAsync_DonorNotFound_ThrowsArgumentException()
    {
        var (service, transplants, patients, _, _, _) = CreateService();
        transplants.GetByIdAsync(TransplantId).Returns(new Transplant { TransplantId = TransplantId, Receiver = new Patient() });
        patients.GetByIdAsync(DonorId).Returns((Patient?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.AssignDonorAsync(TransplantId, DonorId, FinalScore));
    }

    [TestMethod]
    public async Task GetTopMatchesAsDisplayModelsAsync_DonorNotFound_ThrowsInvalidOperationException()
    {
        var (service, _, patients, _, _, _) = CreateService();
        patients.GetByIdAsync(DonorId).Returns((Patient?)null);

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.GetTopMatchesAsDisplayModelsAsync(DonorId, OrganType));
    }

    [TestMethod]
    public async Task GetTopMatchesAsDisplayModelsAsync_DonorNotEligible_ThrowsInvalidOperationException()
    {
        var (service, _, patients, _, _, _) = CreateService();
        patients.GetByIdAsync(DonorId).Returns(new Patient { PatientId = DonorId, IsDonor = false });

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.GetTopMatchesAsDisplayModelsAsync(DonorId, OrganType));
    }

    [TestMethod]
    public async Task IsUrgentAsync_NoHistory_ReturnsFalse()
    {
        var (service, _, _, _, _, histories) = CreateService();
        histories.GetByPatientIdAsync(ReceiverId).Returns((MedicalHistory?)null);

        bool urgent = await service.IsUrgentAsync(ReceiverId);

        Assert.IsFalse(urgent);
    }

    [TestMethod]
    public async Task GetChronicWarningAsync_NoConditions_ReturnsNull()
    {
        var (service, _, _, _, _, histories) = CreateService();
        histories.GetByPatientIdAsync(ReceiverId).Returns(new MedicalHistory());

        string? warning = await service.GetChronicWarningAsync(ReceiverId);

        Assert.IsNull(warning);
    }

    [TestMethod]
    public async Task GetByReceiverIdAsync_FiltersByReceiver()
    {
        var (service, transplants, _, _, _, _) = CreateService();
        transplants.GetAllAsync().Returns(new List<Transplant>
        {
            new() { Receiver = new Patient { PatientId = ReceiverId } },
            new() { Receiver = new Patient { PatientId = ReceiverId + 1 } },
        });

        var result = await service.GetByReceiverIdAsync(ReceiverId);

        Assert.HasCount(1, result);
    }

    private const int UrgentErVisitCount = 10;
    private const int MatchScore = 50;
    private static readonly DateTime DonorDeathDate = new(2020, 1, 1);

    [TestMethod]
    public async Task CreateWaitlistRequestAsync_Valid_CreatesTransplant()
    {
        var (service, transplants, patients, _, _, _) = CreateService();
        patients.GetByIdAsync(ReceiverId).Returns(new Patient { PatientId = ReceiverId });

        await service.CreateWaitlistRequestAsync(ReceiverId, OrganType);

        await transplants.Received().CreateAsync(Arg.Is<Transplant>(transplant => transplant.OrganType == OrganType));
    }

    [TestMethod]
    public async Task AssignDonorAsync_Valid_CompletesTransplant()
    {
        var (service, transplants, patients, _, _, _) = CreateService();
        transplants.GetByIdAsync(TransplantId).Returns(new Transplant { TransplantId = TransplantId, Receiver = new Patient() });
        patients.GetByIdAsync(DonorId).Returns(new Patient { PatientId = DonorId });

        await service.AssignDonorAsync(TransplantId, DonorId, FinalScore);

        await transplants.Received().UpdateAsync(Arg.Is<Transplant>(transplant => transplant.Status == TransplantStatus.Completed));
    }

    [TestMethod]
    public async Task GetAllAsync_ReturnsRepositoryResult()
    {
        var (service, transplants, _, _, _, _) = CreateService();
        transplants.GetAllAsync().Returns(new List<Transplant> { new() { TransplantId = TransplantId, Receiver = new Patient() } });

        var result = await service.GetAllAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetByPatientIdAsync_ReturnsRepositoryResult()
    {
        var (service, transplants, _, _, _, _) = CreateService();
        transplants.GetByPatientIdAsync(ReceiverId).Returns(new List<Transplant> { new() { Receiver = new Patient() } });

        var result = await service.GetByPatientIdAsync(ReceiverId);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task IsUrgentAsync_FrequentRecentErVisits_ReturnsTrue()
    {
        var (service, _, _, records, _, histories) = CreateService();
        histories.GetByPatientIdAsync(ReceiverId).Returns(new MedicalHistory { MedicalHistoryId = TransplantId });
        records.GetByMedicalHistoryIdAsync(TransplantId).Returns(
            Enumerable.Range(0, UrgentErVisitCount)
                .Select(_ => new MedicalRecord { SourceType = SourceType.ER, ConsultationDate = DateTime.UtcNow })
                .ToList());

        bool urgent = await service.IsUrgentAsync(ReceiverId);

        Assert.IsTrue(urgent);
    }

    [TestMethod]
    public async Task GetChronicWarningAsync_WithConditions_ReturnsWarning()
    {
        var (service, _, _, _, _, histories) = CreateService();
        histories.GetByPatientIdAsync(ReceiverId).Returns(new MedicalHistory { ChronicConditions = ["Diabetes"] });

        string? warning = await service.GetChronicWarningAsync(ReceiverId);

        Assert.IsNotNull(warning);
    }

    [TestMethod]
    public async Task GetByDonorIdAsync_FiltersByDonor()
    {
        var (service, transplants, _, _, _, _) = CreateService();
        transplants.GetAllAsync().Returns(new List<Transplant>
        {
            new() { Receiver = new Patient(), Donor = new Patient { PatientId = DonorId } },
            new() { Receiver = new Patient(), Donor = new Patient { PatientId = DonorId + 1 } },
        });

        var result = await service.GetByDonorIdAsync(DonorId);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetTopMatchesAsDisplayModelsAsync_CompatibleReceiver_ReturnsMatch()
    {
        var (service, transplants, patients, records, blood, histories) = CreateService();
        patients.GetByIdAsync(DonorId).Returns(new Patient { PatientId = DonorId, IsDonor = true, DateOfDeath = DonorDeathDate });
        histories.GetByPatientIdAsync(DonorId).Returns(new MedicalHistory { BloodType = BloodType.O, Rh = Rh.Positive });
        histories.GetByPatientIdAsync(ReceiverId).Returns(new MedicalHistory { MedicalHistoryId = TransplantId, BloodType = BloodType.A, Rh = Rh.Positive });
        transplants.GetAllAsync().Returns(new List<Transplant>
        {
            new() { Status = TransplantStatus.Pending, OrganType = OrganType, Receiver = new Patient { PatientId = ReceiverId }, RequestDate = DateTime.UtcNow },
        });
        blood.IsBloodMatch(Arg.Any<BloodType?>(), Arg.Any<BloodType>()).Returns(true);
        blood.IsRhMatch(Arg.Any<Rh?>(), Arg.Any<Rh>()).Returns(true);
        blood.CalculateScore(Arg.Any<Patient>(), Arg.Any<Patient>()).Returns(MatchScore);
        records.GetByMedicalHistoryIdAsync(TransplantId).Returns(new List<MedicalRecord>());

        var result = await service.GetTopMatchesAsDisplayModelsAsync(DonorId, OrganType);

        Assert.HasCount(1, result);
    }
}
