namespace Hospital.Shared.Repositories
{
    public interface INotificationRepository
    {
        void AddNotification(int recipientStaffId, string title, string message);
    }
}
