using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class ModuleRepository(HospitalDbContext context) : IModuleRepository
{
    public async Task<List<Module>> GetAllModulesAsync()
        => await context.Modules.OrderBy(m => m.Id).ToListAsync();

    public async Task<Module?> GetModuleByKeyAsync(string moduleKey)
        => await context.Modules.FirstOrDefaultAsync(m => m.Key == moduleKey);

    public async Task<List<Module>> GetAccessibleModulesByRoleAsync(string roleName)
        => await context.RoleModulePermissions
            .Where(rmp => rmp.Role.Name == roleName)
            .Select(rmp => rmp.Module)
            .OrderBy(m => m.Id)
            .ToListAsync();

    public async Task<bool> RoleHasModuleAccessAsync(string roleName, string moduleKey)
        => await context.RoleModulePermissions
            .AnyAsync(rmp => rmp.Role.Name == roleName && rmp.Module.Key == moduleKey);
}
