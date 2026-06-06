using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using Hospital.Shared.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class ModuleAccessServiceTests
{
    private const int UserId = 3;
    private const string Role = "Doctor";
    private const string ModuleKey = "patients";

    private static (ModuleAccessService Service, IUsersRepository Users, IRolePermissionCache Cache) CreateService()
    {
        var users = Substitute.For<IUsersRepository>();
        var cache = Substitute.For<IRolePermissionCache>();
        return (new ModuleAccessService(users, cache), users, cache);
    }

    [TestMethod]
    public async Task GetAccessibleModulesAsync_UnknownUser_ReturnsEmpty()
    {
        var (service, users, _) = CreateService();
        users.GetUserByIdAsync(UserId).Returns((User?)null);

        var result = await service.GetAccessibleModulesAsync(UserId);

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public async Task GetAccessibleModulesAsync_DisabledUser_ReturnsEmpty()
    {
        var (service, users, _) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId, IsDisabled = true, Role = Role });

        var result = await service.GetAccessibleModulesAsync(UserId);

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public async Task CanAccessModuleAsync_DisabledUser_ReturnsFalse()
    {
        var (service, users, _) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId, IsDisabled = true, Role = Role });

        bool canAccess = await service.CanAccessModuleAsync(UserId, ModuleKey);

        Assert.IsFalse(canAccess);
    }

    [TestMethod]
    public async Task CanAccessModuleAsync_EnabledUser_DelegatesToCache()
    {
        var (service, users, cache) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId, IsDisabled = false, Role = Role });
        cache.RoleHasModuleAccessAsync(Role, ModuleKey).Returns(true);

        bool canAccess = await service.CanAccessModuleAsync(UserId, ModuleKey);

        Assert.IsTrue(canAccess);
    }
}
