using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class PeriodTrackerServiceTests
{
    private const int UserId = 3;
    private const int InvalidUserId = 0;
    private const int NoteId = 1;
    private const double DefaultCycleDays = 28;
    private const string NoteBody = "Drink water";
    private static readonly DateTimeOffset StartDate = new(new DateTime(2026, 6, 1), TimeSpan.Zero);

    private const int CycleLength = 28;
    private const int PeriodLength = 5;
    private const int CurrentMonthOffset = 0;
    private const int FutureMonthOffset = 1;
    private const int PastMonthOffset = -1;
    private const int PmsOptionEarly = 1;
    private const int PmsOptionLate = 3;
    private const float ItemPrice = 10f;

    private static (PeriodTrackerService Service, IUsersRepository Users) CreateService()
    {
        var users = Substitute.For<IUsersRepository>();
        return (new PeriodTrackerService(users), users);
    }

    private static User TrackerUser(int pmsOption = PmsOptionEarly) => new()
    {
        Id = UserId,
        StartPeriodDate = DateOnly.FromDateTime(DateTime.Today),
        CycleDays = CycleLength,
        PeriodLasts = PeriodLength,
        PremenstrualSyndromeOption = pmsOption,
        PeriodNoteEntries = new List<PeriodNote> { new() { NoteId = NoteId, NoteBody = NoteBody } },
    };

    [TestMethod]
    public async Task GetUserAsync_InvalidUserId_ReturnsNull()
    {
        var (service, _) = CreateService();

        var user = await service.GetUserAsync(InvalidUserId);

        Assert.IsNull(user);
    }

    [TestMethod]
    public void GetTrackerState_NoUser_UsesDefaultCycleDays()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns((User?)null);

        var state = service.GetTrackerState(UserId);

        Assert.AreEqual(DefaultCycleDays, state.CycleDays);
    }

    [TestMethod]
    public void GetNotes_InvalidUserId_ReturnsEmpty()
    {
        var (service, _) = CreateService();

        var notes = service.GetNotes(InvalidUserId);

        Assert.IsEmpty(notes);
    }

    [TestMethod]
    public async Task GetNotesAsync_UserWithNotes_ReturnsNoteBody()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User
        {
            Id = UserId,
            PeriodNoteEntries = new List<PeriodNote> { new() { NoteId = NoteId, NoteBody = NoteBody, IsDone = false } },
        });

        var notes = await service.GetNotesAsync(UserId);

        Assert.AreEqual(NoteBody, notes[NoteId].Body);
    }

    [TestMethod]
    public async Task AddNoteAsync_UserNotFound_DoesNotPersist()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns((User?)null);

        await service.AddNoteAsync(UserId, NoteBody);

        await users.DidNotReceive().UpdateUserAsync(Arg.Any<User>());
    }

    [TestMethod]
    public async Task AddNoteAsync_ValidUser_PersistsUser()
    {
        var (service, users) = CreateService();
        var user = new User { Id = UserId, PeriodNoteEntries = new List<PeriodNote>() };
        users.GetUserByIdAsync(UserId).Returns(user);

        await service.AddNoteAsync(UserId, NoteBody);

        await users.Received().UpdateUserAsync(Arg.Is<User>(persisted => persisted.PeriodNoteEntries.Count == 1));
    }

    [TestMethod]
    public async Task UpdatePeriodTrackerAsync_UserNotFound_DoesNotPersist()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns((User?)null);

        await service.UpdatePeriodTrackerAsync(UserId, StartDate, DefaultCycleDays, DefaultCycleDays, 0);

        await users.DidNotReceive().UpdateUserAsync(Arg.Any<User>());
    }

    [TestMethod]
    public async Task DeleteNoteAsync_NoteNotFound_DoesNotPersist()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId, PeriodNoteEntries = new List<PeriodNote>() });

        await service.DeleteNoteAsync(UserId, NoteId);

        await users.DidNotReceive().UpdateUserAsync(Arg.Any<User>());
    }

    [TestMethod]
    public void GetTrackerState_UserWithCycle_HasTracker()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(TrackerUser());

        var state = service.GetTrackerState(UserId);

        Assert.IsTrue(state.HasPeriodTracker);
    }

    [TestMethod]
    public void GetDashboardSnapshot_CurrentCycle_HasTracker()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(TrackerUser());

        var snapshot = service.GetDashboardSnapshot(UserId, CurrentMonthOffset);

        Assert.IsTrue(snapshot.HasPeriodTracker);
    }

    [TestMethod]
    public void GetDashboardSnapshot_CurrentCycle_BuildsPeriodInterval()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(TrackerUser());

        var snapshot = service.GetDashboardSnapshot(UserId, CurrentMonthOffset);

        Assert.IsFalse(string.IsNullOrEmpty(snapshot.PeriodIntervalText));
    }

    [TestMethod]
    public void GetDashboardSnapshot_FutureOffset_BuildsMonthName()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(TrackerUser());

        var snapshot = service.GetDashboardSnapshot(UserId, FutureMonthOffset);

        Assert.IsFalse(string.IsNullOrEmpty(snapshot.CurrentMonthName));
    }

    [TestMethod]
    public void GetDashboardSnapshot_PastOffset_BuildsNextPeriodDate()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(TrackerUser());

        var snapshot = service.GetDashboardSnapshot(UserId, PastMonthOffset);

        Assert.IsFalse(string.IsNullOrEmpty(snapshot.NextPeriodDateString));
    }

    [TestMethod]
    public void GetDashboardSnapshot_LatePmsOption_BuildsPmsInterval()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(TrackerUser(PmsOptionLate));

        var snapshot = service.GetDashboardSnapshot(UserId, CurrentMonthOffset);

        Assert.IsFalse(string.IsNullOrEmpty(snapshot.PmsIntervalText));
    }

    [TestMethod]
    public void GetDashboardSnapshot_WithWellnessItems_PopulatesShopItems()
    {
        var users = Substitute.For<IUsersRepository>();
        users.GetUserByIdAsync(UserId).Returns(TrackerUser());
        var wellness = Substitute.For<Hospital.Shared.Services.IWellnessItemsService>();
        wellness.GetWellnessItems().Returns(new List<Item> { new() { Id = 1, Price = ItemPrice } });
        var service = new PeriodTrackerService(users, wellness);

        var snapshot = service.GetDashboardSnapshot(UserId, CurrentMonthOffset);

        Assert.IsNotEmpty(snapshot.ShopItems);
    }

    [TestMethod]
    public async Task UpdatePeriodTrackerAsync_ValidUser_PersistsChanges()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(TrackerUser());

        await service.UpdatePeriodTrackerAsync(UserId, StartDate, CycleLength, PeriodLength, PmsOptionEarly);

        await users.Received().UpdateUserAsync(Arg.Is<User>(user => user.CycleDays == CycleLength));
    }

    [TestMethod]
    public async Task UpdateNoteAsync_ExistingNote_PersistsChange()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(TrackerUser());

        await service.UpdateNoteAsync(UserId, NoteId, NoteBody, true);

        await users.Received().UpdateUserAsync(Arg.Any<User>());
    }

    [TestMethod]
    public async Task DeleteNoteAsync_ExistingNote_PersistsChange()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(TrackerUser());

        await service.DeleteNoteAsync(UserId, NoteId);

        await users.Received().UpdateUserAsync(Arg.Any<User>());
    }

    [TestMethod]
    public void GetNotes_UserWithNotes_ReturnsNote()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(TrackerUser());

        var notes = service.GetNotes(UserId);

        Assert.AreEqual(NoteBody, notes[NoteId].Body);
    }

    [TestMethod]
    public void UpdatePeriodTracker_ValidUser_PersistsChanges()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(TrackerUser());

        service.UpdatePeriodTracker(UserId, StartDate, CycleLength, PeriodLength, PmsOptionEarly);

        users.Received().UpdateUserAsync(Arg.Any<User>());
    }

    [TestMethod]
    public void AddNote_ValidUser_PersistsNote()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(TrackerUser());

        service.AddNote(UserId, NoteBody);

        users.Received().UpdateUserAsync(Arg.Any<User>());
    }

    [TestMethod]
    public void UpdateNote_ExistingNote_PersistsChange()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(TrackerUser());

        service.UpdateNote(UserId, NoteId, NoteBody, true);

        users.Received().UpdateUserAsync(Arg.Any<User>());
    }

    [TestMethod]
    public void DeleteNote_ExistingNote_PersistsChange()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(TrackerUser());

        service.DeleteNote(UserId, NoteId);

        users.Received().UpdateUserAsync(Arg.Any<User>());
    }

    private const int FollicularStartDaysAgo = 10;
    private const int OvulationStartDaysAgo = 12;
    private const int LutealStartDaysAgo = 20;

    private static User TrackerUserStarting(int daysAgo)
    {
        var user = TrackerUser();
        user.StartPeriodDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-daysAgo));
        return user;
    }

    [TestMethod]
    public void GetDashboardSnapshot_FollicularPhase_SetsPhaseString()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(TrackerUserStarting(FollicularStartDaysAgo));

        var snapshot = service.GetDashboardSnapshot(UserId, CurrentMonthOffset);

        Assert.IsFalse(string.IsNullOrEmpty(snapshot.CurrentPhaseString));
    }

    [TestMethod]
    public void GetDashboardSnapshot_OvulationPhase_SetsOvulationDistance()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(TrackerUserStarting(OvulationStartDaysAgo));

        var snapshot = service.GetDashboardSnapshot(UserId, CurrentMonthOffset);

        Assert.IsFalse(string.IsNullOrEmpty(snapshot.OvulationDistanceString));
    }

    [TestMethod]
    public void GetDashboardSnapshot_LutealPhase_SetsNextPeriodDistance()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(TrackerUserStarting(LutealStartDaysAgo));

        var snapshot = service.GetDashboardSnapshot(UserId, CurrentMonthOffset);

        Assert.IsFalse(string.IsNullOrEmpty(snapshot.NextPeriodDistanceString));
    }

    [TestMethod]
    public async Task GetNotesAsync_InvalidUser_ReturnsEmpty()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns((User?)null);

        var notes = await service.GetNotesAsync(UserId);

        Assert.IsEmpty(notes);
    }
}
