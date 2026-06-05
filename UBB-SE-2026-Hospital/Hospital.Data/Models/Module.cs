using System.Collections.Generic;

namespace Hospital.Data.Models;

public class Module
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<RoleModulePermission> RolePermissions { get; set; } = new List<RoleModulePermission>();
}
