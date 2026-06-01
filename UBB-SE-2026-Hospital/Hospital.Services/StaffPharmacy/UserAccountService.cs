using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class UserAccountService(IUsersRepository usersRepository) : IUserAccountService
{
    public async Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        => await usersRepository.GetAllUsersAsync();

    public async Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
        => await usersRepository.GetUserByIdAsync(userId);

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await usersRepository.GetUserByEmailAsync(email);

    public async Task<bool> UserExistsByIdAsync(int userId, CancellationToken cancellationToken = default)
        => await usersRepository.GetUserByIdAsync(userId) is not null;

    public async Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await usersRepository.GetUserByEmailAsync(email) is not null;

    public async Task<bool> UserHasPeriodTrackerAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await usersRepository.GetUserByIdAsync(userId);
        return user is not null && user.StartPeriodDate != default;
    }

    public async Task CreateUserAsync(string email, string phoneNumber, string passwordHash, string username, bool discountNotifications, bool isDisabled, bool isAdmin, int loyaltyPoints, string role, CancellationToken cancellationToken = default)
    {
        if (await UserExistsByEmailAsync(email, cancellationToken))
        {
            throw new ArgumentException("Email already linked to an account.");
        }

        await usersRepository.CreateUserAsync(new User
        {
            Email = email,
            PhoneNumber = phoneNumber,
            PasswordHash = passwordHash,
            Username = username,
            DiscountNotifications = discountNotifications,
            IsDisabled = isDisabled,
            IsAdmin = isAdmin,
            LoyaltyPoints = loyaltyPoints,
            Role = role,
        });
    }

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
        => await usersRepository.UpdateUserAsync(user);

    public async Task<IReadOnlyList<User>> SearchUsersAsync(string query, CancellationToken cancellationToken = default)
    {
        var users = await usersRepository.GetAllUsersAsync();
        query = query.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            return users;
        }

        if (query.StartsWith("id:", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(query[3..], out int id))
        {
            return users.Where(user => user.Id == id).ToList();
        }

        if (query.StartsWith("username:", StringComparison.OrdinalIgnoreCase))
        {
            string username = query["username:".Length..];
            return users.Where(user => user.Username.Contains(username, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (query.StartsWith("mail:", StringComparison.OrdinalIgnoreCase))
        {
            string email = query["mail:".Length..];
            return users.Where(user => user.Email.Contains(email, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return users.Where(user =>
            user.Email.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            user.Username.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            user.PhoneNumber.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task PromoteToAdminAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await usersRepository.GetUserByIdAsync(userId)
            ?? throw new ArgumentException("User not found.");
        user.IsAdmin = true;
        user.Role = "Admin";
        await usersRepository.UpdateUserAsync(user);
    }

    public async Task DisableAccountAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await usersRepository.GetUserByIdAsync(userId)
            ?? throw new ArgumentException("User not found.");
        user.IsDisabled = true;
        await usersRepository.UpdateUserAsync(user);
    }
}
