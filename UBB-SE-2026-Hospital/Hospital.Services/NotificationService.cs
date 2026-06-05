using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services;

public class NotificationService(
    INotificationRepository notificationRepository,
    IStaffRepository staffRepository) : INotificationService
{
    public async Task CreateNotificationAsync(int recipientStaffId, string title, string message, CancellationToken cancellationToken = default)
    {
        var recipient = await staffRepository.GetByIdAsync(recipientStaffId)
            ?? throw new ArgumentException("Staff member not found.");

        await notificationRepository.CreateAsync(new Notification
        {
            Recipient = recipient,
            Title = title,
            Message = message,
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
        });
    }

    public async Task<IReadOnlyList<Notification>> GetNotificationsForStaffAsync(int staffId, CancellationToken cancellationToken = default)
        => await notificationRepository.GetByStaffIdAsync(staffId);
}
