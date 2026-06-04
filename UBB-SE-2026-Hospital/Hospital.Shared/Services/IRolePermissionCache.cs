using Hospital.Shared.DTOs.Auth;

namespace Hospital.Shared.Services;
public interface IRolePermissionCache
{
    Task<IReadOnlyList<ModuleDto>> GetModulesForRoleAsync(string roleName, CancellationToken cancellationToken = default);

    Task<bool> RoleHasModuleAccessAsync(string roleName, string moduleKey, CancellationToken cancellationToken = default);

    void Invalidate();
}
