using System;
using System.Threading.Tasks;
using Hospital.Services.Auth;
using Hospital.Data.Models;
using Hospital.Shared.DTOs.Auth;
using Hospital.Shared.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class AuthServiceTests
    {
        [TestMethod]
        public async Task LoginAsync_WhenUserNotFound_ThrowsUnauthorizedAccessException()
        {
            var usersRepo = new Mock<Hospital.Data.Repositories.IUsersRepository>();
            var config = new Mock<IConfiguration>();
            usersRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            var sut = new AuthService(usersRepo.Object, config.Object);

            await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(async () => await sut.LoginAsync(new LoginRequest { Email = "a@b.com", Password = "p" }));
        }

        [TestMethod]
        public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
        {
            var usersRepo = new Mock<Hospital.Data.Repositories.IUsersRepository>();
            var config = new Mock<IConfiguration>();
            var user = new Hospital.Data.Models.User { Email = "a@b.com", PasswordHash = "other" };
            usersRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

            var sut = new AuthService(usersRepo.Object, config.Object);

            await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(async () => await sut.LoginAsync(new LoginRequest { Email = "a@b.com", Password = "p" }));
        }

        [TestMethod]
        public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
        {
            var usersRepo = new Mock<Hospital.Data.Repositories.IUsersRepository>();
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c["Jwt:Key"]).Returns("secret_key_for_tests_which_is_ok");
            configuration.Setup(c => c["Jwt:Issuer"]).Returns("issuer");
            configuration.Setup(c => c["Jwt:Audience"]).Returns("audience");
            configuration.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("60");

            var user = new Hospital.Data.Models.User { Email = "a@b.com", PasswordHash = "p", Username = "u", Id = 1, Role = "Client", IsDisabled = false };
            usersRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

            var sut = new AuthService(usersRepo.Object, configuration.Object);

            var result = await sut.LoginAsync(new LoginRequest { Email = "a@b.com", Password = "p" });

            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.Token));
        }
    }
}
