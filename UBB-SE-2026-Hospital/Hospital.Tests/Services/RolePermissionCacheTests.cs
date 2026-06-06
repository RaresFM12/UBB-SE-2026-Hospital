using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class RolePermissionCacheTests
{
    private const string Role = "Doctor";
    private const string AllowedModuleKey = "patients";
    private const string DeniedModuleKey = "billing";

    private static RolePermissionCache CreateCache(IModuleRepository moduleRepository)
    {
        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(IModuleRepository)).Returns(moduleRepository);

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(provider);

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        return new RolePermissionCache(scopeFactory);
    }

    private static IModuleRepository RepositoryWith(params string[] moduleKeys)
    {
        var repository = Substitute.For<IModuleRepository>();
        repository.GetAccessibleModulesByRoleAsync(Role)
            .Returns(moduleKeys.Select(key => new Module { Key = key }).ToList());
        return repository;
    }

    [TestMethod]
    public async Task GetModulesForRoleAsync_MapsRepositoryModules()
    {
        var cache = CreateCache(RepositoryWith(AllowedModuleKey));

        var modules = await cache.GetModulesForRoleAsync(Role);

        Assert.AreEqual(AllowedModuleKey, modules[0].Key);
    }

    [TestMethod]
    public async Task RoleHasModuleAccessAsync_ModulePresent_ReturnsTrue()
    {
        var cache = CreateCache(RepositoryWith(AllowedModuleKey));

        bool hasAccess = await cache.RoleHasModuleAccessAsync(Role, AllowedModuleKey);

        Assert.IsTrue(hasAccess);
    }

    [TestMethod]
    public async Task RoleHasModuleAccessAsync_ModuleAbsent_ReturnsFalse()
    {
        var cache = CreateCache(RepositoryWith(AllowedModuleKey));

        bool hasAccess = await cache.RoleHasModuleAccessAsync(Role, DeniedModuleKey);

        Assert.IsFalse(hasAccess);
    }
}
