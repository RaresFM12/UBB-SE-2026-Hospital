using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IUserAccountService
{
    Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);

    Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> UserExistsByIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> UserHasPeriodTrackerAsync(int userId, CancellationToken cancellationToken = default);

    Task CreateUserAsync(string email, string phoneNumber, string passwordHash, string username, bool discountNotifications, bool isDisabled, bool isAdmin, int loyaltyPoints, string role, CancellationToken cancellationToken = default);

    Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> SearchUsersAsync(string query, CancellationToken cancellationToken = default);

    Task PromoteToAdminAsync(int userId, CancellationToken cancellationToken = default);

    Task DisableAccountAsync(int userId, CancellationToken cancellationToken = default);
}
