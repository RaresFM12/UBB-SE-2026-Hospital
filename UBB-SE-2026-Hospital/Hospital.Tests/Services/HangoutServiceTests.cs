using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class HangoutServiceTests
{
    private const int HangoutId = 51;
    private const int StaffId = 4;
    private const int MaxParticipants = 1;
    private const string ShortTitle = "Hi";
    private const string ValidTitle = "Team Lunch";
    private const string ValidDescription = "A relaxing lunch.";
    private static readonly DateTime FarFutureDate = DateTime.Now.Date.AddDays(30);
    private static readonly DateTime NearDate = DateTime.Now.Date.AddDays(1);

    private static (HangoutService Service, IHangoutRepository Hangouts, IHangoutParticipantRepository Participants, IAppointmentRepository Appointments, IStaffRepository Staff, IEvaluationsRepository Evaluations) CreateService()
    {
        var hangouts = Substitute.For<IHangoutRepository>();
        var participants = Substitute.For<IHangoutParticipantRepository>();
        var appointments = Substitute.For<IAppointmentRepository>();
        var staff = Substitute.For<IStaffRepository>();
        var evaluations = Substitute.For<IEvaluationsRepository>();
        return (new HangoutService(hangouts, participants, appointments, staff, evaluations), hangouts, participants, appointments, staff, evaluations);
    }

    [TestMethod]
    public async Task CreateHangoutAsync_TitleTooShort_ThrowsArgumentException()
    {
        var (service, _, _, _, _, _) = CreateService();

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CreateHangoutAsync(ShortTitle, ValidDescription, FarFutureDate, MaxParticipants));
    }

    [TestMethod]
    public async Task CreateHangoutAsync_DescriptionTooLong_ThrowsArgumentException()
    {
        var (service, _, _, _, _, _) = CreateService();
        string longDescription = new('x', 101);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CreateHangoutAsync(ValidTitle, longDescription, FarFutureDate, MaxParticipants));
    }

    [TestMethod]
    public async Task CreateHangoutAsync_DateTooSoon_ThrowsArgumentException()
    {
        var (service, _, _, _, _, _) = CreateService();

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CreateHangoutAsync(ValidTitle, ValidDescription, NearDate, MaxParticipants));
    }

    [TestMethod]
    public async Task AddParticipantAsync_HangoutNotFound_ThrowsArgumentException()
    {
        var (service, hangouts, _, _, _, _) = CreateService();
        hangouts.GetByIdAsync(HangoutId).Returns((Hangout?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.AddParticipantAsync(HangoutId, StaffId));
    }

    [TestMethod]
    public async Task AddParticipantAsync_StaffNotFound_ThrowsArgumentException()
    {
        var (service, hangouts, _, _, staff, _) = CreateService();
        hangouts.GetByIdAsync(HangoutId).Returns(new Hangout { HangoutID = HangoutId, MaxParticipants = MaxParticipants });
        staff.GetByIdAsync(StaffId).Returns((Staff?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.AddParticipantAsync(HangoutId, StaffId));
    }

    [TestMethod]
    public async Task AddParticipantAsync_HangoutFull_ThrowsInvalidOperationException()
    {
        var (service, hangouts, participants, _, staff, _) = CreateService();
        var hangout = new Hangout { HangoutID = HangoutId, MaxParticipants = MaxParticipants };
        hangouts.GetByIdAsync(HangoutId).Returns(hangout);
        staff.GetByIdAsync(StaffId).Returns(new Staff { StaffId = StaffId });
        participants.GetByHangoutIdAsync(HangoutId).Returns(new List<HangoutParticipant>
        {
            new() { Hangout = hangout, Staff = new Staff { StaffId = StaffId + 1 } },
        });

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => service.AddParticipantAsync(HangoutId, StaffId));
    }

    private const int RoomyMaxParticipants = 5;

    [TestMethod]
    public async Task GetAllHangoutsAsync_ReturnsHangoutsWithParticipants()
    {
        var (service, hangouts, participants, _, _, _) = CreateService();
        hangouts.GetAllAsync().Returns(new List<Hangout> { new() { HangoutID = HangoutId } });
        participants.GetByHangoutIdAsync(Arg.Any<int>()).Returns(new List<HangoutParticipant>());

        var result = await service.GetAllHangoutsAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetHangoutByIdAsync_ReturnsRepositoryResult()
    {
        var (service, hangouts, _, _, _, _) = CreateService();
        hangouts.GetByIdAsync(HangoutId).Returns(new Hangout { HangoutID = HangoutId });

        var result = await service.GetHangoutByIdAsync(HangoutId);

        Assert.AreEqual(HangoutId, result!.HangoutID);
    }

    [TestMethod]
    public async Task CreateHangoutAsync_Valid_ReturnsHangoutId()
    {
        var (service, hangouts, _, _, _, _) = CreateService();
        hangouts.CreateAsync(Arg.Any<Hangout>()).Returns(new Hangout { HangoutID = HangoutId });

        int result = await service.CreateHangoutAsync(ValidTitle, ValidDescription, FarFutureDate, RoomyMaxParticipants);

        Assert.AreEqual(HangoutId, result);
    }

    [TestMethod]
    public async Task AddParticipantAsync_Valid_PersistsParticipant()
    {
        var (service, hangouts, participants, appointments, staff, evaluations) = CreateService();
        hangouts.GetByIdAsync(HangoutId).Returns(new Hangout { HangoutID = HangoutId, MaxParticipants = RoomyMaxParticipants, Date = FarFutureDate });
        staff.GetByIdAsync(StaffId).Returns(new Staff { StaffId = StaffId });
        participants.GetByHangoutIdAsync(HangoutId).Returns(new List<HangoutParticipant>());
        appointments.GetByDoctorIdAsync(StaffId).Returns(new List<Appointment>());
        evaluations.GetByDoctorIdAsync(StaffId).Returns(new List<MedicalEvaluation>());

        await service.AddParticipantAsync(HangoutId, StaffId);

        await participants.Received().CreateAsync(Arg.Any<HangoutParticipant>());
    }
}
