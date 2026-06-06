using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class SalaryComputationServiceTests
{
    private const double DoctorBaseHourlyRate = 85.0;
    private const double ShiftHours = 8.0;
    private const double SurgeonSpecializationBonusPercentage = 0.20;
    private const double SalaryTolerance = 0.0001;

    private const int DoctorStaffId = 1;
    private const int Month = 1;
    private const int Year = 2024;
    private const string SurgeonSpecialization = "Surgeon";

    private static readonly DateTime ShiftStart = new(2024, 1, 3, 9, 0, 0);
    private static readonly DateTime ShiftEnd = new(2024, 1, 3, 17, 0, 0);

    private const double ExpectedBaseSalary = ShiftHours * DoctorBaseHourlyRate;
    private const double ExpectedSurgeonSalary = ExpectedBaseSalary + ExpectedBaseSalary * SurgeonSpecializationBonusPercentage;

    [TestMethod]
    public async Task ComputeSalaryDoctorAsync_WithoutBonuses_ReturnsBaseSalary()
    {
        var service = CreateService();
        var doctor = CreateDoctor(string.Empty);

        double salary = await service.ComputeSalaryDoctorAsync(doctor, CreateMonthlyShifts(), Month, Year);

        Assert.AreEqual(ExpectedBaseSalary, salary, SalaryTolerance);
    }

    [TestMethod]
    public async Task ComputeSalaryDoctorAsync_SurgeonSpecialization_AddsSpecializationBonus()
    {
        var service = CreateService();
        var doctor = CreateDoctor(SurgeonSpecialization);

        double salary = await service.ComputeSalaryDoctorAsync(doctor, CreateMonthlyShifts(), Month, Year);

        Assert.AreEqual(ExpectedSurgeonSalary, salary, SalaryTolerance);
    }

    private static SalaryComputationService CreateService()
        => new(null!, null!, null!, null!, new FakeHangoutParticipantRepository());

    private static Doctor CreateDoctor(string specialization)
        => new()
        {
            StaffId = DoctorStaffId,
            Specialization = specialization,
            YearsOfExperience = 0,
        };

    private static IReadOnlyList<Shift> CreateMonthlyShifts()
        => [new Shift { StartTime = ShiftStart, EndTime = ShiftEnd }];

    private sealed class FakeHangoutParticipantRepository : IHangoutParticipantRepository
    {
        public Task<HangoutParticipant?> GetByIdAsync(int hangoutId, int staffId) => throw new NotImplementedException();
        public Task<List<HangoutParticipant>> GetByHangoutIdAsync(int hangoutId) => throw new NotImplementedException();
        public Task<List<HangoutParticipant>> GetByStaffIdAsync(int staffId) => Task.FromResult<List<HangoutParticipant>>([]);
        public Task<HangoutParticipant> CreateAsync(HangoutParticipant participant) => throw new NotImplementedException();
        public Task DeleteAsync(int hangoutId, int staffId) => throw new NotImplementedException();
    }

    private const double PharmacistBaseHourlyRate = 45.0;
    private const double ExpectedPharmacistBaseSalary = ShiftHours * PharmacistBaseHourlyRate;

    private static (SalaryComputationService Service, IStaffRepository Staff, IShiftRepository Shifts, IPharmacyHandoverRepository Handovers, IHangoutRepository Hangouts, IHangoutParticipantRepository Participants) CreateServiceWithMocks()
    {
        var staff = Substitute.For<IStaffRepository>();
        var shifts = Substitute.For<IShiftRepository>();
        var handovers = Substitute.For<IPharmacyHandoverRepository>();
        var hangouts = Substitute.For<IHangoutRepository>();
        var participants = Substitute.For<IHangoutParticipantRepository>();
        participants.GetByStaffIdAsync(Arg.Any<int>()).Returns(new List<HangoutParticipant>());
        return (new SalaryComputationService(staff, shifts, handovers, hangouts, participants), staff, shifts, handovers, hangouts, participants);
    }

    [TestMethod]
    public async Task ComputeSalaryPharmacistAsync_WithoutBonuses_ReturnsBaseSalary()
    {
        var (service, _, _, handovers, _, _) = CreateServiceWithMocks();
        handovers.GetAllAsync().Returns(new List<PharmacyHandover>());
        var pharmacist = new Pharmacyst { StaffId = DoctorStaffId, YearsOfExperience = 0 };

        double salary = await service.ComputeSalaryPharmacistAsync(pharmacist, CreateMonthlyShifts(), Month, Year);

        Assert.AreEqual(ExpectedPharmacistBaseSalary, salary, SalaryTolerance);
    }

    [TestMethod]
    public async Task GetAllStaffAsync_ReturnsRepositoryResult()
    {
        var (service, staff, _, _, _, _) = CreateServiceWithMocks();
        staff.GetAllAsync().Returns(new List<Staff> { new() { StaffId = DoctorStaffId } });

        var result = await service.GetAllStaffAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetAllShiftsAsync_ReturnsRepositoryResult()
    {
        var (service, _, shifts, _, _, _) = CreateServiceWithMocks();
        shifts.GetAllAsync().Returns(new List<Shift> { new() { Id = 1 } });

        var result = await service.GetAllShiftsAsync();

        Assert.HasCount(1, result);
    }

    private const double NightShiftMultiplier = 1.20;
    private const double SaturdayMultiplier = 1.15;
    private const double NightShiftHours = 3.0;
    private const double ExperienceBonusPerYear = 0.02;
    private const double HangoutParticipationMultiplier = 1.05;
    private const int ExperienceYears = 5;
    private const int SoldMedicinesBonusThreshold = 10;
    private static readonly DateTime NightShiftStart = new(2024, 1, 3, 20, 0, 0);
    private static readonly DateTime NightShiftEnd = new(2024, 1, 3, 23, 0, 0);
    private static readonly DateTime SaturdayShiftStart = new(2024, 1, 6, 9, 0, 0);
    private static readonly DateTime SaturdayShiftEnd = new(2024, 1, 6, 17, 0, 0);

    private const double ExpectedNightSalary = NightShiftHours * DoctorBaseHourlyRate * NightShiftMultiplier;
    private const double ExpectedSaturdaySalary = ShiftHours * DoctorBaseHourlyRate * SaturdayMultiplier;
    private const double ExpectedExperiencedSalary = ExpectedBaseSalary + ExpectedBaseSalary * (ExperienceYears * ExperienceBonusPerYear);
    private const double ExpectedParticipationSalary = ExpectedBaseSalary * HangoutParticipationMultiplier;

    [TestMethod]
    public async Task ComputeSalaryDoctorAsync_NightShift_AppliesNightMultiplier()
    {
        var service = CreateService();
        double salary = await service.ComputeSalaryDoctorAsync(
            CreateDoctor(string.Empty),
            [new Shift { StartTime = NightShiftStart, EndTime = NightShiftEnd }],
            Month, Year);

        Assert.AreEqual(ExpectedNightSalary, salary, SalaryTolerance);
    }

    [TestMethod]
    public async Task ComputeSalaryDoctorAsync_SaturdayShift_AppliesWeekendMultiplier()
    {
        var service = CreateService();
        double salary = await service.ComputeSalaryDoctorAsync(
            CreateDoctor(string.Empty),
            [new Shift { StartTime = SaturdayShiftStart, EndTime = SaturdayShiftEnd }],
            Month, Year);

        Assert.AreEqual(ExpectedSaturdaySalary, salary, SalaryTolerance);
    }

    [TestMethod]
    public async Task ComputeSalaryDoctorAsync_ExperiencedDoctor_AddsExperienceBonus()
    {
        var service = CreateService();
        var doctor = new Doctor { StaffId = DoctorStaffId, Specialization = string.Empty, YearsOfExperience = ExperienceYears };

        double salary = await service.ComputeSalaryDoctorAsync(doctor, CreateMonthlyShifts(), Month, Year);

        Assert.AreEqual(ExpectedExperiencedSalary, salary, SalaryTolerance);
    }

    [TestMethod]
    public async Task ComputeSalaryDoctorAsync_HangoutParticipant_AppliesParticipationBonus()
    {
        var (service, _, _, _, hangouts, participants) = CreateServiceWithMocks();
        participants.GetByStaffIdAsync(DoctorStaffId).Returns(new List<HangoutParticipant>
        {
            new() { Hangout = new Hangout { HangoutID = 1, Date = new DateTime(Year, Month, 15) } },
        });
        hangouts.GetAllAsync().Returns(new List<Hangout> { new() { HangoutID = 1, Date = new DateTime(Year, Month, 15) } });

        double salary = await service.ComputeSalaryDoctorAsync(CreateDoctor(string.Empty), CreateMonthlyShifts(), Month, Year);

        Assert.AreEqual(ExpectedParticipationSalary, salary, SalaryTolerance);
    }

    [TestMethod]
    public async Task ComputeSalaryPharmacistAsync_WithMedicineSales_AddsSalesBonus()
    {
        var (service, _, _, handovers, _, _) = CreateServiceWithMocks();
        handovers.GetAllAsync().Returns(Enumerable.Range(0, SoldMedicinesBonusThreshold)
            .Select(_ => new PharmacyHandover { Pharmacist = new Pharmacyst { StaffId = DoctorStaffId }, HandoverDate = new DateTime(Year, Month, 15) })
            .ToList());
        var pharmacist = new Pharmacyst { StaffId = DoctorStaffId, YearsOfExperience = 0 };

        double salary = await service.ComputeSalaryPharmacistAsync(pharmacist, CreateMonthlyShifts(), Month, Year);

        Assert.IsGreaterThan(ExpectedPharmacistBaseSalary, salary);
    }
}
