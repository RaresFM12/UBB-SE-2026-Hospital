using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class ModuleRepository(HospitalDbContext context) : IModuleRepository
{
    public async Task<List<Module>> GetAllModulesAsync()
        => await context.Modules.OrderBy(module => module.Id).ToListAsync();

    public async Task<Module?> GetModuleByKeyAsync(string moduleKey)
        => await context.Modules.FirstOrDefaultAsync(module => module.Key == moduleKey);

    public async Task<List<Module>> GetAccessibleModulesByRoleAsync(string roleName)
        => await context.RoleModulePermissions
            .Where(roleModulePermissions => roleModulePermissions.Role.Name == roleName)
            .Select(roleModulePermissions => roleModulePermissions.Module)
            .OrderBy(module => module.Id)
            .ToListAsync();

    public async Task<bool> RoleHasModuleAccessAsync(string roleName, string moduleKey)
        => await context.RoleModulePermissions
            .AnyAsync(roleModulePermissions => roleModulePermissions.Role.Name == roleName && roleModulePermissions.Module.Key == moduleKey);
}
