using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models.Auth;

namespace Hospital.Data.Repositories;

public interface IModuleRepository
{
    Task<List<Module>> GetAllModulesAsync();
    Task<Module?> GetModuleByKeyAsync(string moduleKey);
    Task<List<Module>> GetAccessibleModulesByRoleAsync(string roleName);
    Task<bool> RoleHasModuleAccessAsync(string roleName, string moduleKey);
}
