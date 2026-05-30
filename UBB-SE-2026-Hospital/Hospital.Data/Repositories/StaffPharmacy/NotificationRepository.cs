using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class NotificationRepository(HospitalDbContext context) : INotificationRepository
{
    public async Task<Notification?> GetByIdAsync(int notificationId)
        => await context.Notifications.FindAsync(notificationId);

    public async Task<List<Notification>> GetAllAsync()
        => await context.Notifications.ToListAsync();

    public async Task<List<Notification>> GetByStaffIdAsync(int staffId)
        => await context.Notifications.Where(n => n.Recipient!.StaffId == staffId).ToListAsync();

    public async Task<List<Notification>> GetUnreadByStaffIdAsync(int staffId)
        => await context.Notifications
            .Where(n => n.Recipient!.StaffId == staffId && !n.IsRead)
            .ToListAsync();

    public async Task<Notification> CreateAsync(Notification notification)
    {
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();
        return notification;
    }

    public async Task<Notification> UpdateAsync(Notification notification)
    {
        context.Notifications.Update(notification);
        await context.SaveChangesAsync();
        return notification;
    }

    public async Task DeleteAsync(int notificationId)
    {
        var notification = await context.Notifications.FindAsync(notificationId);
        if (notification is not null)
        {
            context.Notifications.Remove(notification);
            await context.SaveChangesAsync();
        }
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await context.Notifications.FindAsync(notificationId);
        if (notification is not null)
        {
            notification.IsRead = true;
            await context.SaveChangesAsync();
        }
    }
}
