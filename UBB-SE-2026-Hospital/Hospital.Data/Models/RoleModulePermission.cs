namespace Hospital.Data.Models;

public class RoleModulePermission
{
    public int RoleId { get; set; }
    public int ModuleId { get; set; }

    public Role Role { get; set; } = null!;
    public Module Module { get; set; } = null!;
}
