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
            .Include(u => u.UserDiscountEntries)
            .Include(u => u.UserNotificationEntries)
            .Include(u => u.PeriodNoteEntries)
            .FirstOrDefaultAsync(u => u.Id == userId);

    public async Task<User?> GetUserByEmailAsync(string email)
        => await context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetUserByUsernameAsync(string username)
        => await context.Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<bool> ExistsByUsernameAsync(string username)
        => await context.Users.AnyAsync(u => u.Username == username);

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
            .Include(u => u.UserNotificationEntries)
            .Where(u => u.UserNotificationEntries.Any())
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
        => await context.UserDiscounts.Where(d => d.User.Id == userId).ToListAsync();

    public async Task AddUserDiscountAsync(int userId, UserDiscount discount)
    {
        var user = await context.Users.FindAsync(userId);
        discount.User = user!;
        context.UserDiscounts.Add(discount);
        await context.SaveChangesAsync();
    }

    public async Task<List<UserNotification>> GetUserNotificationsAsync(int userId)
        => await context.UserNotifications.Where(n => n.User.Id == userId).ToListAsync();

    public async Task AddUserNotificationAsync(int userId, UserNotification notification)
    {
        var user = await context.Users.FindAsync(userId);
        notification.User = user!;
        context.UserNotifications.Add(notification);
        await context.SaveChangesAsync();
    }

    public async Task<List<PeriodNote>> GetPeriodNotesAsync(int userId)
        => await context.PeriodNotes.Where(n => n.User.Id == userId).ToListAsync();

    public async Task AddPeriodNoteAsync(int userId, PeriodNote note)
    {
        var user = await context.Users.FindAsync(userId);
        note.User = user!;
        context.PeriodNotes.Add(note);
        await context.SaveChangesAsync();
    }
}
