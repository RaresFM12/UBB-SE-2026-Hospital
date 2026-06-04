using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services.Auth;
using Hospital.Shared.DTOs.Auth;
using Hospital.Shared.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Hospital.Tests.Integration
{
    // Integration tests for ModuleAccessService: real UsersRepository + EF Core, with a
    // mocked IRolePermissionCache (external collaborator).
    // Two tests per public service function.
    [TestClass]
    public sealed class ModuleAccessServiceIntegrationTests
    {
        private static async Task<User> SeedUserAsync(
            Hospital.Data.HospitalDbContext context, string role = "Doctor", bool disabled = false)
        {
            var user = new User
            {
                Email = "u@test.com",
                Username = "user1",
                PasswordHash = "hash",
                Role = role,
                IsDisabled = disabled,
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        // ---- GetAccessibleModulesAsync ----
        [TestMethod]
        public async Task GetAccessibleModulesAsync_WhenUserActive_ReturnsModulesFromCache()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            User user = await SeedUserAsync(context, "Doctor");
            var cache = new Mock<IRolePermissionCache>();
            cache.Setup(c => c.GetModulesForRoleAsync("Doctor", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ModuleDto> { new ModuleDto { Key = "patients" } });
            var service = new ModuleAccessService(new UsersRepository(context), cache.Object);

            IReadOnlyList<ModuleDto> result = await service.GetAccessibleModulesAsync(user.Id);

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task GetAccessibleModulesAsync_WhenUserDisabled_ReturnsEmpty()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            User user = await SeedUserAsync(context, "Doctor", disabled: true);
            var cache = new Mock<IRolePermissionCache>();
            var service = new ModuleAccessService(new UsersRepository(context), cache.Object);

            IReadOnlyList<ModuleDto> result = await service.GetAccessibleModulesAsync(user.Id);

            Assert.AreEqual(0, result.Count);
        }

        // ---- CanAccessModuleAsync ----
        [TestMethod]
        public async Task CanAccessModuleAsync_WhenCacheGrantsAccess_ReturnsTrue()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            User user = await SeedUserAsync(context, "Doctor");
            var cache = new Mock<IRolePermissionCache>();
            cache.Setup(c => c.RoleHasModuleAccessAsync("Doctor", "patients", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            var service = new ModuleAccessService(new UsersRepository(context), cache.Object);

            Assert.IsTrue(await service.CanAccessModuleAsync(user.Id, "patients"));
        }

        [TestMethod]
        public async Task CanAccessModuleAsync_WhenUserMissing_ReturnsFalse()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var cache = new Mock<IRolePermissionCache>();
            var service = new ModuleAccessService(new UsersRepository(context), cache.Object);

            Assert.IsFalse(await service.CanAccessModuleAsync(999, "patients"));
        }
    }
}
