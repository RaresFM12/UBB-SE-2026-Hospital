using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class AuthServiceTests
{
    private const string Email = "user@hospital.test";
    private const string Password = "Str0ng!Pass";
    private const string WrongPassword = "Wr0ng!Pass";
    private const string SigningKey = "ThisIsASuperSecretTestSigningKey1234567890";
    private const string IssuerKey = "Jwt:Issuer";
    private const string AudienceKey = "Jwt:Audience";
    private const string ExpiryKey = "Jwt:ExpiryMinutes";
    private const string SigningKeyKey = "Jwt:Key";

    private static (AuthService Service, IUsersRepository Users) CreateService()
    {
        var users = Substitute.For<IUsersRepository>();
        var configuration = Substitute.For<IConfiguration>();
        configuration[SigningKeyKey].Returns(SigningKey);
        configuration[IssuerKey].Returns((string?)null);
        configuration[AudienceKey].Returns((string?)null);
        configuration[ExpiryKey].Returns((string?)null);
        return (new AuthService(users, configuration), users);
    }

    private static LoginRequest Request(string password = Password)
        => new() { Email = Email, Password = password };

    [TestMethod]
    public async Task LoginAsync_UnknownEmail_ThrowsUnauthorized()
    {
        var (service, users) = CreateService();
        users.GetUserByEmailAsync(Email).Returns((User?)null);

        await Assert.ThrowsExactlyAsync<UnauthorizedAccessException>(
            () => service.LoginAsync(Request()));
    }

    [TestMethod]
    public async Task LoginAsync_DisabledUser_ThrowsUnauthorized()
    {
        var (service, users) = CreateService();
        users.GetUserByEmailAsync(Email).Returns(new User { Email = Email, PasswordHash = Password, IsDisabled = true });

        await Assert.ThrowsExactlyAsync<UnauthorizedAccessException>(
            () => service.LoginAsync(Request()));
    }

    [TestMethod]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
    {
        var (service, users) = CreateService();
        users.GetUserByEmailAsync(Email).Returns(new User { Email = Email, PasswordHash = Password, IsDisabled = false });

        await Assert.ThrowsExactlyAsync<UnauthorizedAccessException>(
            () => service.LoginAsync(Request(WrongPassword)));
    }

    [TestMethod]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        var (service, users) = CreateService();
        users.GetUserByEmailAsync(Email).Returns(new User { Email = Email, Username = Email, PasswordHash = Password, IsDisabled = false });

        var response = await service.LoginAsync(Request());

        Assert.IsFalse(string.IsNullOrWhiteSpace(response.Token));
    }
}
