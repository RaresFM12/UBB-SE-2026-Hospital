using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class UserAccountServiceTests
{
    private const int UserId = 3;
    private const string Email = "user@hospital.test";
    private const string Username = "ana_pop";
    private const string PhoneNumber = "0712345678";
    private const string ValidPassword = "Str0ng!Pass";
    private const string WeakPassword = "weak";
    private const string InvalidEmail = "not-an-email";
    private const string Role = "Client";

    private static (UserAccountService Service, IUsersRepository Users) CreateService()
    {
        var users = Substitute.For<IUsersRepository>();
        return (new UserAccountService(users), users);
    }

    [TestMethod]
    public async Task UserExistsByIdAsync_NotFound_ReturnsFalse()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns((User?)null);

        bool exists = await service.UserExistsByIdAsync(UserId);

        Assert.IsFalse(exists);
    }

    [TestMethod]
    public async Task CreateUserAsync_EmailAlreadyUsed_ThrowsArgumentException()
    {
        var (service, users) = CreateService();
        users.GetUserByEmailAsync(Email).Returns(new User { Id = UserId, Email = Email });

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CreateUserAsync(Email, PhoneNumber, ValidPassword, Username, false, false, false, 0, Role));
    }

    [TestMethod]
    public async Task PromoteToAdminAsync_UserNotFound_ThrowsArgumentException()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns((User?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.PromoteToAdminAsync(UserId));
    }

    [TestMethod]
    public async Task DisableAccountAsync_UserNotFound_ThrowsArgumentException()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns((User?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.DisableAccountAsync(UserId));
    }

    [TestMethod]
    public void Register_InvalidEmail_ThrowsArgumentException()
    {
        var (service, _) = CreateService();

        Assert.ThrowsExactly<ArgumentException>(
            () => service.Register(InvalidEmail, ValidPassword, ValidPassword, Username, PhoneNumber, Role));
    }

    [TestMethod]
    public void Register_PasswordMismatch_ThrowsArgumentException()
    {
        var (service, _) = CreateService();

        Assert.ThrowsExactly<ArgumentException>(
            () => service.Register(Email, ValidPassword, WeakPassword, Username, PhoneNumber, Role));
    }

    [TestMethod]
    public void Register_WeakPassword_ThrowsArgumentException()
    {
        var (service, _) = CreateService();

        Assert.ThrowsExactly<ArgumentException>(
            () => service.Register(Email, WeakPassword, WeakPassword, Username, PhoneNumber, Role));
    }

    [TestMethod]
    public void Login_UnknownEmail_ThrowsArgumentException()
    {
        var (service, users) = CreateService();
        users.GetUserByEmailAsync(Email).Returns((User?)null);

        Assert.ThrowsExactly<ArgumentException>(() => service.Login(Email, ValidPassword));
    }

    [TestMethod]
    public async Task SearchUsersAsync_EmptyQuery_ReturnsAllUsers()
    {
        var (service, users) = CreateService();
        users.GetAllUsersAsync().Returns(new List<User> { new() { Id = UserId } });

        var result = await service.SearchUsersAsync("   ");

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task SearchUsersAsync_IdPrefix_FiltersById()
    {
        var (service, users) = CreateService();
        users.GetAllUsersAsync().Returns(new List<User>
        {
            new() { Id = UserId },
            new() { Id = UserId + 1 },
        });

        var result = await service.SearchUsersAsync($"id:{UserId}");

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetUserByEmailAsync_ReturnsRepositoryResult()
    {
        var (service, users) = CreateService();
        users.GetUserByEmailAsync(Email).Returns(new User { Id = UserId, Email = Email });

        var result = await service.GetUserByEmailAsync(Email);

        Assert.AreEqual(UserId, result!.Id);
    }

    [TestMethod]
    public async Task GetAllUsersAsync_ReturnsRepositoryResult()
    {
        var (service, users) = CreateService();
        users.GetAllUsersAsync().Returns(new List<User> { new() { Id = UserId } });

        var result = await service.GetAllUsersAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task UserExistsByEmailAsync_KnownEmail_ReturnsTrue()
    {
        var (service, users) = CreateService();
        users.GetUserByEmailAsync(Email).Returns(new User { Email = Email });

        bool exists = await service.UserExistsByEmailAsync(Email);

        Assert.IsTrue(exists);
    }

    [TestMethod]
    public async Task UserHasPeriodTrackerAsync_UserWithStartDate_ReturnsTrue()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId });

        bool hasTracker = await service.UserHasPeriodTrackerAsync(UserId);

        Assert.IsTrue(hasTracker);
    }

    [TestMethod]
    public async Task CreateUserAsync_NewEmail_CreatesUser()
    {
        var (service, users) = CreateService();
        users.GetUserByEmailAsync(Email).Returns((User?)null);

        await service.CreateUserAsync(Email, PhoneNumber, ValidPassword, Username, false, false, false, 0, Role);

        await users.Received().CreateUserAsync(Arg.Is<User>(user => user.Email == Email));
    }

    [TestMethod]
    public async Task UpdateUserAsync_DelegatesToRepository()
    {
        var (service, users) = CreateService();

        await service.UpdateUserAsync(new User { Id = UserId });

        await users.Received().UpdateUserAsync(Arg.Any<User>());
    }

    [TestMethod]
    public async Task PromoteToAdminAsync_Existing_SetsAdmin()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId });

        await service.PromoteToAdminAsync(UserId);

        await users.Received().UpdateUserAsync(Arg.Is<User>(user => user.IsAdmin));
    }

    [TestMethod]
    public async Task DisableAccountAsync_Existing_SetsDisabled()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId });

        await service.DisableAccountAsync(UserId);

        await users.Received().UpdateUserAsync(Arg.Is<User>(user => user.IsDisabled));
    }

    [TestMethod]
    public async Task SearchUsersAsync_UsernamePrefix_FiltersByUsername()
    {
        var (service, users) = CreateService();
        users.GetAllUsersAsync().Returns(new List<User>
        {
            new() { Id = UserId, Username = Username },
            new() { Id = UserId + 1, Username = "other" },
        });

        var result = await service.SearchUsersAsync($"username:{Username}");

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task SearchUsersAsync_MailPrefix_FiltersByEmail()
    {
        var (service, users) = CreateService();
        users.GetAllUsersAsync().Returns(new List<User>
        {
            new() { Id = UserId, Email = Email },
            new() { Id = UserId + 1, Email = "other@x.test" },
        });

        var result = await service.SearchUsersAsync($"mail:{Email}");

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void Register_ValidData_CreatesUser()
    {
        var (service, users) = CreateService();
        users.GetUserByEmailAsync(Email).Returns((User?)null);

        service.Register(Email, ValidPassword, ValidPassword, Username, PhoneNumber, Role);

        users.Received().CreateUserAsync(Arg.Is<User>(user => user.Email == Email));
    }

    [TestMethod]
    public void Login_ValidCredentials_SetsCurrentUser()
    {
        var (service, users) = CreateService();
        users.GetUserByEmailAsync(Email).Returns(new User { Id = UserId, Email = Email, PasswordHash = ValidPassword });

        service.Login(Email, ValidPassword);

        Assert.AreEqual(UserId, service.CurrentUser!.Id);
    }

    [TestMethod]
    public void Login_DisabledUser_ThrowsArgumentException()
    {
        var (service, users) = CreateService();
        users.GetUserByEmailAsync(Email).Returns(new User { Id = UserId, Email = Email, PasswordHash = ValidPassword, IsDisabled = true });

        Assert.ThrowsExactly<ArgumentException>(() => service.Login(Email, ValidPassword));
    }

    [TestMethod]
    public void LoadCurrentUser_SetsAndReturnsUser()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId });

        var result = service.LoadCurrentUser(UserId);

        Assert.AreEqual(UserId, result!.Id);
    }

    [TestMethod]
    public void UpdateProfile_WithCurrentUser_PersistsChanges()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId });
        service.LoadCurrentUser(UserId);

        service.UpdateProfile(Username, PhoneNumber);

        users.Received().UpdateUserAsync(Arg.Is<User>(user => user.Username == Username));
    }

    [TestMethod]
    public void ChangePassword_WeakPassword_ThrowsArgumentException()
    {
        var (service, _) = CreateService();

        Assert.ThrowsExactly<ArgumentException>(
            () => service.ChangePassword(ValidPassword, WeakPassword, WeakPassword));
    }

    [TestMethod]
    public void ChangePassword_Mismatch_ThrowsArgumentException()
    {
        var (service, _) = CreateService();

        Assert.ThrowsExactly<ArgumentException>(
            () => service.ChangePassword(ValidPassword, ValidPassword, WeakPassword));
    }

    [TestMethod]
    public void ChangePassword_Valid_PersistsNewPassword()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId });
        service.LoadCurrentUser(UserId);

        service.ChangePassword(ValidPassword, ValidPassword, ValidPassword);

        users.Received().UpdateUserAsync(Arg.Is<User>(user => user.PasswordHash == ValidPassword));
    }

    [TestMethod]
    public async Task GetUserByIdAsync_ReturnsRepositoryResult()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId });

        var result = await service.GetUserByIdAsync(UserId);

        Assert.AreEqual(UserId, result!.Id);
    }

    [TestMethod]
    public async Task UserExistsByIdAsync_KnownUser_ReturnsTrue()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId });

        bool exists = await service.UserExistsByIdAsync(UserId);

        Assert.IsTrue(exists);
    }

    [TestMethod]
    public void UpdateProfile_NoCurrentUser_DoesNotPersist()
    {
        var (service, users) = CreateService();

        service.UpdateProfile(Username, PhoneNumber);

        users.DidNotReceive().UpdateUserAsync(Arg.Any<User>());
    }

    [TestMethod]
    public void SearchUsers_ReturnsMatches()
    {
        var (service, users) = CreateService();
        users.GetAllUsersAsync().Returns(new List<User> { new() { Id = UserId } });

        var result = service.SearchUsers("   ");

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void PromoteToAdmin_Existing_SetsAdmin()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId });

        service.PromoteToAdmin(new User { Id = UserId });

        users.Received().UpdateUserAsync(Arg.Is<User>(user => user.IsAdmin));
    }

    [TestMethod]
    public void DisableAccount_Existing_SetsDisabled()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId });

        service.DisableAccount(new User { Id = UserId });

        users.Received().UpdateUserAsync(Arg.Is<User>(user => user.IsDisabled));
    }

    [TestMethod]
    public void Login_WrongPassword_ThrowsArgumentException()
    {
        var (service, users) = CreateService();
        users.GetUserByEmailAsync(Email).Returns(new User { Id = UserId, Email = Email, PasswordHash = ValidPassword });

        Assert.ThrowsExactly<ArgumentException>(() => service.Login(Email, WeakPassword));
    }

    [TestMethod]
    public void Register_InvalidPhoneNumber_ThrowsArgumentException()
    {
        var (service, _) = CreateService();

        Assert.ThrowsExactly<ArgumentException>(
            () => service.Register(Email, ValidPassword, ValidPassword, Username, "phone!!", Role));
    }

    [TestMethod]
    public void Register_InvalidUsername_ThrowsArgumentException()
    {
        var (service, _) = CreateService();

        Assert.ThrowsExactly<ArgumentException>(
            () => service.Register(Email, ValidPassword, ValidPassword, "bad name 1", PhoneNumber, Role));
    }

    [TestMethod]
    public void ChangePassword_NoCurrentUser_DoesNotPersist()
    {
        var (service, users) = CreateService();

        service.ChangePassword(ValidPassword, ValidPassword, ValidPassword);

        users.DidNotReceive().UpdateUserAsync(Arg.Any<User>());
    }

    [TestMethod]
    public async Task UserHasPeriodTrackerAsync_UnknownUser_ReturnsFalse()
    {
        var (service, users) = CreateService();
        users.GetUserByIdAsync(UserId).Returns((User?)null);

        bool hasTracker = await service.UserHasPeriodTrackerAsync(UserId);

        Assert.IsFalse(hasTracker);
    }
}
