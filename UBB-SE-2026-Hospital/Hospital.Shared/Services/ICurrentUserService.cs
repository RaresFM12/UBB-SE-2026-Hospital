using Hospital.Data.Models;
using Hospital.Shared.Enums;

namespace Hospital.Shared.Services;

public interface ICurrentUserService
{
    int UserId { get; set; }

    string Role { get; }

    UserRole RoleType { get; set; }

    void SetFromUser(User user);
}
