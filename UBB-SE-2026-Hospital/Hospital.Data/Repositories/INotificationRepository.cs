using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(int notificationId);
    Task<List<Notification>> GetAllAsync();
    Task<List<Notification>> GetByStaffIdAsync(int staffId);
    Task<List<Notification>> GetUnreadByStaffIdAsync(int staffId);
    Task<Notification> CreateAsync(Notification notification);
    Task<Notification> UpdateAsync(Notification notification);
    Task DeleteAsync(int notificationId);
    Task MarkAsReadAsync(int notificationId);
}
