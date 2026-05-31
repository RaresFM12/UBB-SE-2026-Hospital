using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class NotificationService : INotificationService
{
    public Task CreateNotificationAsync(int recipientStaffId, string title, string message, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<Notification>> GetNotificationsForStaffAsync(int staffId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
