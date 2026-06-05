using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services;

public class UserAccountService(IUsersRepository usersRepository) : IUserAccountService
{
    public User? CurrentUser { get; private set; }

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

    public List<User> SearchUsers(string query) =>
        SearchUsersAsync(query).GetAwaiter().GetResult().ToList();

    public async Task PromoteToAdminAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await usersRepository.GetUserByIdAsync(userId)
            ?? throw new ArgumentException("User not found.");
        user.IsAdmin = true;
        user.Role = "Admin";
        await usersRepository.UpdateUserAsync(user);
    }

    public void PromoteToAdmin(User user) =>
        PromoteToAdminAsync(user.Id).GetAwaiter().GetResult();

    public async Task DisableAccountAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await usersRepository.GetUserByIdAsync(userId)
            ?? throw new ArgumentException("User not found.");
        user.IsDisabled = true;
        await usersRepository.UpdateUserAsync(user);
    }

    public void DisableAccount(User user) =>
        DisableAccountAsync(user.Id).GetAwaiter().GetResult();

    public User? LoadCurrentUser(int userId)
    {
        CurrentUser = GetUserByIdAsync(userId).GetAwaiter().GetResult();
        return CurrentUser;
    }

    public void Register(string email, string password, string confirmPassword, string username, string phoneNumber, string role)
    {
        if (!IsValidEmailFormat(email))
        {
            throw new ArgumentException("Not a valid email format. Must be <text>@<text>.<text>");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be empty.");
        }

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            throw new ArgumentException("Passwords do not match.");
        }

        if (!IsValidPasswordFormat(password))
        {
            throw new ArgumentException("Password must be 8+ characters and include uppercase, lowercase, a digit, and a special character (!@#%^*).");
        }

        if (!string.IsNullOrWhiteSpace(username) && !IsValidUsernameFormat(username))
        {
            throw new ArgumentException("Username must contain only letters and underscores.");
        }

        phoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? string.Empty : phoneNumber;
        if (!string.IsNullOrEmpty(phoneNumber) && !IsValidPhoneNumberFormat(phoneNumber))
        {
            throw new ArgumentException("Phone number must contain only digits.");
        }

        var existingUser = usersRepository.GetUserByEmailAsync(email).GetAwaiter().GetResult();
        if (existingUser is not null)
        {
            throw new ArgumentException("Email is already linked to an account.");
        }

        CreateUserAsync(
            email,
            phoneNumber,
            password,
            username ?? string.Empty,
            false,
            false,
            string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase),
            0,
            role).GetAwaiter().GetResult();
    }

    public void Login(string email, string password)
    {
        var user = usersRepository.GetUserByEmailAsync(email).GetAwaiter().GetResult()
            ?? throw new ArgumentException("Invalid email or password.");

        if (user.IsDisabled || !string.Equals(user.PasswordHash, password, StringComparison.Ordinal))
        {
            throw new ArgumentException("Invalid email or password.");
        }

        CurrentUser = user;
    }

    public void UpdateProfile(string username, string phoneNumber)
    {
        if (CurrentUser is null)
        {
            return;
        }

        CurrentUser.Username = username;
        CurrentUser.PhoneNumber = phoneNumber;
        UpdateUserAsync(CurrentUser).GetAwaiter().GetResult();
    }

    public void ChangePassword(string oldPassword, string newPassword, string confirmPassword)
    {
        if (!IsValidPasswordFormat(newPassword))
        {
            throw new ArgumentException("Password must be 8+ characters and include uppercase, lowercase, a digit, and a special character (!@#%^*).");
        }

        if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
        {
            throw new ArgumentException("Passwords do not match.");
        }

        if (CurrentUser is null)
        {
            return;
        }

        CurrentUser.PasswordHash = newPassword;
        UpdateUserAsync(CurrentUser).GetAwaiter().GetResult();
    }

    private static bool IsValidEmailFormat(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        return System.Text.RegularExpressions.Regex.IsMatch(email.Trim(), @"^.+@.+\..+");
    }

    private static bool IsValidPasswordFormat(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        return System.Text.RegularExpressions.Regex.IsMatch(
            password,
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#%^*])[A-Za-z\d!@#%^*]{8,}$");
    }

    private static bool IsValidUsernameFormat(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        return System.Text.RegularExpressions.Regex.IsMatch(username.Trim(), @"^[A-Za-z_]+$");
    }

    private static bool IsValidPhoneNumberFormat(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return false;
        }

        return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber.Trim(), @"^[0-9]+$");
    }
}
