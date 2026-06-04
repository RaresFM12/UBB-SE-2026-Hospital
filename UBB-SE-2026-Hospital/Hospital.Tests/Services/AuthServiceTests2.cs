using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Hospital.Services.Auth;
using Hospital.Data.Repositories;
using Hospital.Data.Models;
using Moq;
using Microsoft.Extensions.Configuration;
using Hospital.Shared.DTOs.Auth;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class AuthServiceTests2
    {
        [TestMethod]
        public async Task LoginAsync_DisabledUser_Throws()
        {
            var users = new Mock<IUsersRepository>();
            var cfg = new Mock<IConfiguration>();
            var user = new User { Email = "x@x.com", PasswordHash = "p", IsDisabled = true };
            users.Setup(u => u.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            var svc = new AuthService(users.Object, cfg.Object);
            await Assert.ThrowsExceptionAsync<System.UnauthorizedAccessException>(async () => await svc.LoginAsync(new LoginRequest { Email = "x@x.com", Password = "p" }));
        }
    }
}
