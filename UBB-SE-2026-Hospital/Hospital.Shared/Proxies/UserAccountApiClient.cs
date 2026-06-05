using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class UserAccountApiClient(HttpClient httpClient) : ApiClientBase(httpClient), IUserAccountService, IUserAccountApiClient
{
    private const string BaseUri = "api/users";

    public User? CurrentUser { get; private set; }

    public User? LoadCurrentUser(int userId)
    {
        var user = Task.Run(async () => await GetAsync<User>("api/auth/me")).GetAwaiter().GetResult();
        CurrentUser = user;
        return user;
    }

    public void Login(string email, string password)
        => throw new NotSupportedException("Use AuthClient for login.");

    public void Register(string email, string password, string confirmPassword, string username, string phoneNumber, string role = "Client")
        => Task.Run(async () => await PostAsync<object, object>(BaseUri, new { email, password, confirmPassword, username, phoneNumber, role })).GetAwaiter().GetResult();

    public void UpdateProfile(string newUsername, string newPhoneNumber)
    {
        if (CurrentUser == null) return;
        Task.Run(async () => await PutAsync(
            "api/auth/profile",
            new { username = newUsername, phoneNumber = newPhoneNumber })).GetAwaiter().GetResult();
        CurrentUser.Username = newUsername;
        CurrentUser.PhoneNumber = newPhoneNumber;
    }

    public void ChangePassword(string oldPassword, string newPassword, string confirmNewPassword)
        => throw new NotSupportedException("Password changes are handled by AuthController.");

    public List<User> SearchUsers(string query)
    {
        var url = string.IsNullOrWhiteSpace(query) ? BaseUri : $"{BaseUri}/search?q={Uri.EscapeDataString(query)}";
        return Task.Run(async () => await GetAsync<List<User>>(url) ?? []).GetAwaiter().GetResult();
    }

    public void PromoteToAdmin(User client)
        => Task.Run(async () => await PostAsync<object, object>($"{BaseUri}/{client.Id}/promote", new { })).GetAwaiter().GetResult();

    public void DisableAccount(User client)
        => Task.Run(async () => await PostAsync<object, object>($"{BaseUri}/{client.Id}/disable", new { })).GetAwaiter().GetResult();

    public void Logout() => CurrentUser = null;

    public async Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<User>>(BaseUri) ?? [];

    public async Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
        => await GetAsync<User>($"{BaseUri}/{userId}");

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await GetAsync<User>($"{BaseUri}/by-email?email={Uri.EscapeDataString(email)}");

    public async Task<bool> UserExistsByIdAsync(int userId, CancellationToken cancellationToken = default)
        => await GetAsync<bool>($"{BaseUri}/{userId}/exists");

    public async Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await GetAsync<bool>($"{BaseUri}/exists?email={Uri.EscapeDataString(email)}");

    public async Task<bool> UserHasPeriodTrackerAsync(int userId, CancellationToken cancellationToken = default)
        => await GetAsync<bool>($"{BaseUri}/{userId}/has-period-tracker");

    public async Task CreateUserAsync(string email, string phoneNumber, string passwordHash, string username, bool discountNotifications, bool isDisabled, bool isAdmin, int loyaltyPoints, string role, CancellationToken cancellationToken = default)
        => await PostAsync<object, object>(BaseUri, new { email, phoneNumber, passwordHash, username, discountNotifications, isDisabled, isAdmin, loyaltyPoints, role });

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
        => await PutAsync($"{BaseUri}/{user.Id}", user);

    public async Task<IReadOnlyList<User>> SearchUsersAsync(string query, CancellationToken cancellationToken = default)
    {
        var url = string.IsNullOrWhiteSpace(query) ? BaseUri : $"{BaseUri}/search?q={Uri.EscapeDataString(query)}";
        return await GetAsync<List<User>>(url) ?? [];
    }

    public async Task PromoteToAdminAsync(int userId, CancellationToken cancellationToken = default)
        => await PostAsync<object, object>($"{BaseUri}/{userId}/promote", new { });

    public async Task DisableAccountAsync(int userId, CancellationToken cancellationToken = default)
        => await PostAsync<object, object>($"{BaseUri}/{userId}/disable", new { });
}


