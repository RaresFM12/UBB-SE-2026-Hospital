using Hospital.Data.Models;

namespace Hospital.Shared.Proxies
{
    public interface IUserAccountApiClient
    {
        User? CurrentUser { get; }

        void ChangePassword(string oldPassword, string newPassword, string confirmNewPassword);
        Task CreateUserAsync(string email, string phoneNumber, string passwordHash, string username, bool discountNotifications, bool isDisabled, bool isAdmin, int loyaltyPoints, string role, CancellationToken cancellationToken = default);
        void DisableAccount(User client);
        Task DisableAccountAsync(int userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
        User? LoadCurrentUser(int userId);
        void Login(string email, string password);
        void Logout();
        void PromoteToAdmin(User client);
        Task PromoteToAdminAsync(int userId, CancellationToken cancellationToken = default);
        void Register(string email, string password, string confirmPassword, string username, string phoneNumber, string role = "Client");
        List<User> SearchUsers(string query);
        Task<IReadOnlyList<User>> SearchUsersAsync(string query, CancellationToken cancellationToken = default);
        void UpdateProfile(string newUsername, string newPhoneNumber);
        Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);
        Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> UserExistsByIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> UserHasPeriodTrackerAsync(int userId, CancellationToken cancellationToken = default);
    }
}