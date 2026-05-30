using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface IUsersRepository
{
    Task<User?> GetUserByIdAsync(int userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<bool> ExistsByUsernameAsync(string username);
    Task<List<User>> GetAllUsersAsync();
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(User user);
    Task DeleteUserAsync(int userId);

    Task<List<User>> GetUsersWithDueNotificationsAsync();
    Task UpdateLoyaltyPointsAsync(int userId, int points);
    Task<List<UserDiscount>> GetUserDiscountsAsync(int userId);
    Task AddUserDiscountAsync(int userId, UserDiscount discount);
    Task<List<UserNotification>> GetUserNotificationsAsync(int userId);
    Task AddUserNotificationAsync(int userId, UserNotification notification);
    Task<List<PeriodNote>> GetPeriodNotesAsync(int userId);
    Task AddPeriodNoteAsync(int userId, PeriodNote note);
}
