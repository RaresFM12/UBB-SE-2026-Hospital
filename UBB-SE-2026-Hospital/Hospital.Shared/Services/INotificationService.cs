using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface INotificationService
{
    Task CreateNotificationAsync(int recipientStaffId, string title, string message, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Notification>> GetNotificationsForStaffAsync(int staffId, CancellationToken cancellationToken = default);
}
