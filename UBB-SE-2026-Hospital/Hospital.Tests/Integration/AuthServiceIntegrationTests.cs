using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services.Auth;
using Hospital.Shared.DTOs.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Hospital.Tests.Integration
{
    // Integration tests for AuthService: real UsersRepository + EF Core, with a mocked
    // IConfiguration (external collaborator providing JWT settings).
    // Two tests per public service function.
    [TestClass]
    public sealed class AuthServiceIntegrationTests
    {
        private static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password), salt, 100_000, HashAlgorithmName.SHA256, 32);
            byte[] combined = new byte[48];
            Buffer.BlockCopy(salt, 0, combined, 0, 16);
            Buffer.BlockCopy(hash, 0, combined, 16, 32);
            return Convert.ToBase64String(combined);
        }

        private static IConfiguration BuildConfiguration()
        {
            var config = new Mock<IConfiguration>();
            config.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("60");
            config.Setup(c => c["Jwt:Key"]).Returns("super-secret-signing-key-with-enough-length-1234567890");
            config.Setup(c => c["Jwt:Issuer"]).Returns("HospitalTests");
            config.Setup(c => c["Jwt:Audience"]).Returns("HospitalTests");
            return config.Object;
        }

        private static async Task<User> SeedUserAsync(
            Hospital.Data.HospitalDbContext context, string email, string password, bool disabled = false)
        {
            var user = new User
            {
                Email = email,
                Username = "user1",
                PasswordHash = HashPassword(password),
                Role = "Doctor",
                IsDisabled = disabled,
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        // ---- LoginAsync ----
        [TestMethod]
        public async Task LoginAsync_WhenValidCredentials_ReturnsTokenResponse()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await SeedUserAsync(context, "doc@test.com", "Passw0rd!");
            var service = new AuthService(new UsersRepository(context), BuildConfiguration());

            AuthResponse response = await service.LoginAsync(
                new LoginRequest { Email = "doc@test.com", Password = "Passw0rd!" });

            Assert.IsFalse(string.IsNullOrWhiteSpace(response.Token));
            Assert.IsTrue(response.ExpiresAtUtc > DateTime.UtcNow);
        }

        [TestMethod]
        public async Task LoginAsync_WhenWrongPassword_ThrowsUnauthorized()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await SeedUserAsync(context, "doc@test.com", "Passw0rd!");
            var service = new AuthService(new UsersRepository(context), BuildConfiguration());

            await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
                async () => await service.LoginAsync(
                    new LoginRequest { Email = "doc@test.com", Password = "wrong" }));
        }
    }
}
