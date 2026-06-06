using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class BloodCompatibilityServiceTests
{
    private const int ExactBloodRhMatchScore = 50;
    private const int FullAgeScore = 30;
    private const int SameSexScore = 20;
    private const int ExpectedExactSameSexSameAgeScore = ExactBloodRhMatchScore + FullAgeScore + SameSexScore;
    private const int ExpectedNoCompatibilityScore = 0;

    private static readonly DateTime SharedBirthDate = new(1990, 1, 1);

    private static BloodCompatibilityService CreateService()
        => new(null!, null!);

    [TestMethod]
    public void IsBloodMatch_TypeODonor_ToAnyReceiver_ReturnsTrue()
    {
        var service = CreateService();

        bool isMatch = service.IsBloodMatch(BloodType.O, BloodType.A);

        Assert.IsTrue(isMatch);
    }

    [TestMethod]
    public void IsBloodMatch_TypeADonor_ToTypeBReceiver_ReturnsFalse()
    {
        var service = CreateService();

        bool isMatch = service.IsBloodMatch(BloodType.A, BloodType.B);

        Assert.IsFalse(isMatch);
    }

    [TestMethod]
    public void IsBloodMatch_NullDonor_ReturnsFalse()
    {
        var service = CreateService();

        bool isMatch = service.IsBloodMatch(null, BloodType.AB);

        Assert.IsFalse(isMatch);
    }

    [TestMethod]
    public void IsRhMatch_NegativeDonor_ToNegativeReceiver_ReturnsTrue()
    {
        var service = CreateService();

        bool isMatch = service.IsRhMatch(Rh.Negative, Rh.Negative);

        Assert.IsTrue(isMatch);
    }

    [TestMethod]
    public void IsRhMatch_PositiveDonor_ToNegativeReceiver_ReturnsFalse()
    {
        var service = CreateService();

        bool isMatch = service.IsRhMatch(Rh.Positive, Rh.Negative);

        Assert.IsFalse(isMatch);
    }

    [TestMethod]
    public void IsRhMatch_PositiveDonor_ToPositiveReceiver_ReturnsTrue()
    {
        var service = CreateService();

        bool isMatch = service.IsRhMatch(Rh.Positive, Rh.Positive);

        Assert.IsTrue(isMatch);
    }

    [TestMethod]
    public void CalculateScore_ExactMatchSameSexSameAge_ReturnsMaximumScore()
    {
        var service = CreateService();
        var donor = CreatePatient(BloodType.A, Rh.Positive, Sex.F);
        var recipient = CreatePatient(BloodType.A, Rh.Positive, Sex.F);

        int score = service.CalculateScore(donor, recipient);

        Assert.AreEqual(ExpectedExactSameSexSameAgeScore, score);
    }

    [TestMethod]
    public void CalculateScore_DonorWithoutMedicalHistory_ReturnsZero()
    {
        var service = CreateService();
        var donor = CreatePatient(BloodType.A, Rh.Positive, Sex.F);
        donor.MedicalHistory = null;
        var recipient = CreatePatient(BloodType.A, Rh.Positive, Sex.F);

        int score = service.CalculateScore(donor, recipient);

        Assert.AreEqual(ExpectedNoCompatibilityScore, score);
    }

    private static Patient CreatePatient(BloodType bloodType, Rh rh, Sex sex)
        => new()
        {
            DateOfBirth = SharedBirthDate,
            Sex = sex,
            MedicalHistory = new MedicalHistory
            {
                BloodType = bloodType,
                Rh = rh,
            },
        };

    private const int RecipientId = 1;
    private const int DonorId = 2;

    [TestMethod]
    public async Task GetTopCompatibleDonorsAsync_RecipientWithoutBloodType_ReturnsEmpty()
    {
        var patients = Substitute.For<IPatientRepository>();
        var histories = Substitute.For<IMedicalHistoryRepository>();
        patients.GetByIdAsync(RecipientId).Returns(new Patient { PatientId = RecipientId });
        histories.GetByPatientIdAsync(RecipientId).Returns(new MedicalHistory());
        var service = new BloodCompatibilityService(patients, histories);

        var result = await service.GetTopCompatibleDonorsAsync(RecipientId);

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public async Task GetTopCompatibleDonorsAsync_CompatibleDonor_ReturnsDonor()
    {
        var patients = Substitute.For<IPatientRepository>();
        var histories = Substitute.For<IMedicalHistoryRepository>();
        var recipient = new Patient { PatientId = RecipientId, DateOfBirth = SharedBirthDate, Sex = Sex.F };
        var donor = new Patient { PatientId = DonorId, DateOfBirth = SharedBirthDate, Sex = Sex.F };
        patients.GetByIdAsync(RecipientId).Returns(recipient);
        patients.GetAllAsync().Returns(new List<Patient> { recipient, donor });
        histories.GetByPatientIdAsync(RecipientId).Returns(new MedicalHistory { BloodType = BloodType.A, Rh = Rh.Positive });
        histories.GetByPatientIdAsync(DonorId).Returns(new MedicalHistory { BloodType = BloodType.O, Rh = Rh.Positive });
        var service = new BloodCompatibilityService(patients, histories);

        var result = await service.GetTopCompatibleDonorsAsync(RecipientId);

        Assert.HasCount(1, result);
    }
}
