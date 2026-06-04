namespace Hospital.Shared.Services
{
    using Hospital.Shared.Models;

    public interface ICurrentUserService
    {
        int UserId { get; set; }

        string Role { get; }

        UserRole RoleType { get; set; }

        void SetFromUser(User user);
    }
}
