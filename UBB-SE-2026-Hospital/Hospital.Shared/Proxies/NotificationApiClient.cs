using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class NotificationApiClient : ApiClientBase, INotificationService
{
    public NotificationApiClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public Task CreateNotificationAsync(int recipientStaffId, string title, string message, CancellationToken cancellationToken = default)
    {
        return PostAsync(
            "api/notifications",
            new { RecipientStaffId = recipientStaffId, Title = title, Message = message },
            cancellationToken);
    }

    public Task<IReadOnlyList<Notification>> GetNotificationsForStaffAsync(int staffId, CancellationToken cancellationToken = default)
    {
        return Task.FromException<IReadOnlyList<Notification>>(
            new NotSupportedException("Listing notifications is not available in the desktop client."));
    }
}
