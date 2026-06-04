using Hospital.Shared.DTOs.Auth;

namespace Hospital.Shared.Services;

public interface IModuleAccessService
{
    Task<IReadOnlyList<ModuleDto>> GetAccessibleModulesAsync(int userId, CancellationToken cancellationToken = default);

    Task<bool> CanAccessModuleAsync(int userId, string moduleKey, CancellationToken cancellationToken = default);
}
