using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Hospital.Services;
using Hospital.Data.Repositories;
using Hospital.Data.Models;
using Microsoft.Extensions.Configuration;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class AuthService_LoginTests
    {
        [TestMethod]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            var mockUsersRepo = new Mock<IUsersRepository>();
            var inMemoryConfig = new Mock<IConfiguration>();

            var testUser = new User { Id = 1, Username = "tester", Email = "t@t.com", PasswordHash = "p", Role = "User", IsDisabled = false };
            mockUsersRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(testUser);
            inMemoryConfig.Setup(c => c["Jwt:Key"]).Returns("ReplaceWith256BitKeyReplaceWith256BitKey");
            inMemoryConfig.Setup(c => c["Jwt:Issuer"]).Returns("Hospital.API");
            inMemoryConfig.Setup(c => c["Jwt:Audience"]).Returns("Hospital.Clients");
            inMemoryConfig.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("60");

            var service = new AuthService(mockUsersRepo.Object, inMemoryConfig.Object);

            var response = await service.LoginAsync(new LoginRequest { Email = "t@t.com", Password = "p" });

            Assert.IsFalse(string.IsNullOrWhiteSpace(response.Token));
        }

        [TestMethod]
        public async Task Login_DisabledUser_ThrowsUnauthorized()
        {
            var mockUsersRepo = new Mock<IUsersRepository>();
            var inMemoryConfig = new Mock<IConfiguration>();

            var disabledUser = new User { Id = 2, Email = "d@d.com", PasswordHash = "p", IsDisabled = true };
            mockUsersRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(disabledUser);

            var service = new AuthService(mockUsersRepo.Object, inMemoryConfig.Object);

            await Assert.ThrowsExceptionAsync(typeof(UnauthorizedAccessException), async () => await service.LoginAsync(new LoginRequest { Email = "d@d.com", Password = "p" }));
        }

        [TestMethod]
        public async Task Login_InvalidPassword_ThrowsUnauthorized()
        {
            var mockUsersRepo = new Mock<IUsersRepository>();
            var inMemoryConfig = new Mock<IConfiguration>();

            // Stored hash is plain 'stored', password provided is different => fallback FixedTimeEquals will be false
            var userWithDifferentHash = new User { Id = 3, Email = "x@x.com", PasswordHash = "stored", IsDisabled = false };
            mockUsersRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(userWithDifferentHash);

            var service = new AuthService(mockUsersRepo.Object, inMemoryConfig.Object);

            await Assert.ThrowsExceptionAsync(typeof(UnauthorizedAccessException), async () => await service.LoginAsync(new LoginRequest { Email = "x@x.com", Password = "wrong" }));
        }
    }
}
