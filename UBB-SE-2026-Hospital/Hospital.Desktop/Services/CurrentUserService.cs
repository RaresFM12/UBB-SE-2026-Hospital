using Hospital.Data.Models;
using Hospital.Shared.Enums;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Services;

public class CurrentUserService : ICurrentUserService
{
    public int UserId { get; set; }
    public string Role { get; private set; } = string.Empty;
    public UserRole RoleType { get; set; }

    public void SetFromUser(User user)
    {
        UserId = user.Id;
        Role = user.Role ?? string.Empty;
        RoleType = Enum.TryParse<UserRole>(user.Role, true, out var parsed) ? parsed : UserRole.Client;
    }
}
