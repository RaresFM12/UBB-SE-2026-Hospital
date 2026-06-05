using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IUserAccountService
{
    User? CurrentUser { get; }
    Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> UserExistsByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> UserHasPeriodTrackerAsync(int userId, CancellationToken cancellationToken = default);
    Task CreateUserAsync(string email, string phoneNumber, string passwordHash, string username, bool discountNotifications, bool isDisabled, bool isAdmin, int loyaltyPoints, string role, CancellationToken cancellationToken = default);
    Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> SearchUsersAsync(string query, CancellationToken cancellationToken = default);
    List<User> SearchUsers(string query);
    Task PromoteToAdminAsync(int userId, CancellationToken cancellationToken = default);
    void PromoteToAdmin(User user);
    Task DisableAccountAsync(int userId, CancellationToken cancellationToken = default);
    void DisableAccount(User user);
    User? LoadCurrentUser(int userId);
    void Register(string email, string password, string confirmPassword, string username, string phoneNumber, string role);
    void UpdateProfile(string username, string phoneNumber);
    void ChangePassword(string oldPassword, string newPassword, string confirmPassword);
    void Login(string email, string password);
}
