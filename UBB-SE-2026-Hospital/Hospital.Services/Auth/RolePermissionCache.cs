using System.Collections.Concurrent;
using Hospital.Data.Repositories;
using Hospital.Shared.DTOs.Auth;
using Hospital.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Hospital.Services.Auth;

// Singleton cache of role -> accessible modules. Entries are loaded lazily on
// first use and refreshed after a short TTL.
public sealed class RolePermissionCache(IServiceScopeFactory scopeFactory) : IRolePermissionCache
{
    private static readonly TimeSpan TimeToLive = TimeSpan.FromMinutes(5);

    private readonly ConcurrentDictionary<string, CacheEntry> cache = new(StringComparer.OrdinalIgnoreCase);

    public Task<IReadOnlyList<ModuleDto>> GetModulesForRoleAsync(string roleName, CancellationToken cancellationToken = default)
    {
        CacheEntry entry = cache.AddOrUpdate(
            roleName,
            key => new CacheEntry(LoadAsync(key)),
            (key, existing) => existing.IsExpired ? new CacheEntry(LoadAsync(key)) : existing);

        return entry.Modules;
    }

    public async Task<bool> RoleHasModuleAccessAsync(string roleName, string moduleKey, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ModuleDto> modules = await GetModulesForRoleAsync(roleName, cancellationToken);
        return modules.Any(m => string.Equals(m.Key, moduleKey, StringComparison.OrdinalIgnoreCase));
    }

    public void Invalidate() => cache.Clear();

    private async Task<IReadOnlyList<ModuleDto>> LoadAsync(string roleName)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        IModuleRepository repository = scope.ServiceProvider.GetRequiredService<IModuleRepository>();

        var modules = await repository.GetAccessibleModulesByRoleAsync(roleName);
        return modules
            .Select(m => new ModuleDto { Id = m.Id, Key = m.Key, Name = m.Name, Description = m.Description })
            .ToList();
    }

    private sealed class CacheEntry(Task<IReadOnlyList<ModuleDto>> modules)
    {
        private readonly DateTime expiresAtUtc = DateTime.UtcNow.Add(TimeToLive);

        public Task<IReadOnlyList<ModuleDto>> Modules { get; } = modules;

        public bool IsExpired => DateTime.UtcNow >= expiresAtUtc;
    }
}
