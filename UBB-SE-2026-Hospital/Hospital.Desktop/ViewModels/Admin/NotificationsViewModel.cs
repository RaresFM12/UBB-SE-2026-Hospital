using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.Admin;

public partial class NotificationsViewModel : ObservableObject
{
    private readonly INotificationService notificationService;

    [ObservableProperty] private string recipientStaffId = string.Empty;
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string message = string.Empty;
    [ObservableProperty] private string statusMessage = string.Empty;

    public NotificationsViewModel(INotificationService notificationService)
    {
        this.notificationService = notificationService;
    }

    [RelayCommand]
    private void Load()
    {
    }

    [RelayCommand]
    private async Task SendAsync()
    {
        StatusMessage = string.Empty;
        if (!int.TryParse(RecipientStaffId, out int staffId))
        {
            StatusMessage = "Error: Recipient Staff Id must be a valid number.";
            return;
        }

        try
        {
            await notificationService.CreateNotificationAsync(staffId, Title, Message);
            StatusMessage = $"Notification sent to staff #{staffId}.";
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
