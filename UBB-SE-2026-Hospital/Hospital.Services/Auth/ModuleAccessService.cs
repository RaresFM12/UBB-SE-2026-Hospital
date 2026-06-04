using Hospital.Data.Repositories;
using Hospital.Shared.DTOs.Auth;
using Hospital.Shared.Services;

namespace Hospital.Services.Auth;

public class ModuleAccessService(IUsersRepository usersRepository, IRolePermissionCache rolePermissionCache) : IModuleAccessService
{
    public async Task<IReadOnlyList<ModuleDto>> GetAccessibleModulesAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await usersRepository.GetUserByIdAsync(userId);
        if (user is null || user.IsDisabled)
            return [];

        return await rolePermissionCache.GetModulesForRoleAsync(user.Role, cancellationToken);
    }

    public async Task<bool> CanAccessModuleAsync(int userId, string moduleKey, CancellationToken cancellationToken = default)
    {
        var user = await usersRepository.GetUserByIdAsync(userId);
        if (user is null || user.IsDisabled)
            return false;

        return await rolePermissionCache.RoleHasModuleAccessAsync(user.Role, moduleKey, cancellationToken);
    }
}
