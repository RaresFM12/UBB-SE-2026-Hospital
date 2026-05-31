using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class UserAccountService : IUserAccountService
{
    public Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> UserExistsByIdAsync(int userId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> UserHasPeriodTrackerAsync(int userId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task CreateUserAsync(string email, string phoneNumber, string passwordHash, string username, bool discountNotifications, bool isDisabled, bool isAdmin, int loyaltyPoints, string role, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<User>> SearchUsersAsync(string query, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task PromoteToAdminAsync(int userId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task DisableAccountAsync(int userId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
