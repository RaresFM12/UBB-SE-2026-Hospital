using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class UsersRepository(HospitalDbContext context) : IUsersRepository
{
    public async Task<User?> GetUserByIdAsync(int userId)
        => await context.Users
            .Include(user => user.UserDiscountEntries)
            .Include(user => user.UserNotificationEntries)
            .Include(user => user.PeriodNoteEntries)
            .FirstOrDefaultAsync(user => user.Id == userId);

    public async Task<User?> GetUserByEmailAsync(string email)
        => await context.Users.FirstOrDefaultAsync(user => user.Email == email);

    public async Task<User?> GetUserByUsernameAsync(string username)
        => await context.Users.FirstOrDefaultAsync(user => user.Username == username);

    public async Task<bool> ExistsByUsernameAsync(string username)
        => await context.Users.AnyAsync(user => user.Username == username);

    public async Task<List<User>> GetAllUsersAsync()
        => await context.Users.ToListAsync();

    public async Task<User> CreateUserAsync(User user)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteUserAsync(int userId)
    {
        var user = await context.Users.FindAsync(userId);
        if (user is not null)
        {
            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<User>> GetUsersWithDueNotificationsAsync()
        => await context.Users
            .Include(user => user.UserNotificationEntries)
            .Where(user => user.UserNotificationEntries.Any())
            .ToListAsync();

    public async Task UpdateLoyaltyPointsAsync(int userId, int points)
    {
        var user = await context.Users.FindAsync(userId);
        if (user is not null)
        {
            user.LoyaltyPoints = points;
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<UserDiscount>> GetUserDiscountsAsync(int userId)
        => await context.UserDiscounts
            .Include(discount => discount.Item)
            .Where(discount => discount.User.Id == userId)
            .ToListAsync();

    public async Task AddUserDiscountAsync(int userId, UserDiscount discount)
    {
        var user = await context.Users.FindAsync(userId);
        discount.User = user!;
        context.UserDiscounts.Add(discount);
        await context.SaveChangesAsync();
    }

    public async Task<List<UserNotification>> GetUserNotificationsAsync(int userId)
        => await context.UserNotifications.Where(notification => notification.User.Id == userId).ToListAsync();

    public async Task AddUserNotificationAsync(int userId, UserNotification notification)
    {
        var user = await context.Users.FindAsync(userId);
        notification.User = user!;
        context.UserNotifications.Add(notification);
        await context.SaveChangesAsync();
    }

    public async Task<List<PeriodNote>> GetPeriodNotesAsync(int userId)
        => await context.PeriodNotes.Where(notification => notification.User.Id == userId).ToListAsync();

    public async Task AddPeriodNoteAsync(int userId, PeriodNote note)
    {
        var user = await context.Users.FindAsync(userId);
        note.User = user!;
        context.PeriodNotes.Add(note);
        await context.SaveChangesAsync();
    }
}
