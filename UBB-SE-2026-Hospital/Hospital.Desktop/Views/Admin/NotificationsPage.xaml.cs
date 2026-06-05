using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Admin;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.Admin;

public sealed partial class NotificationsPage : Page
{
    public NotificationsViewModel ViewModel { get; private set; } = null!;

    public NotificationsPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new NotificationsViewModel(App.Services.GetRequiredService<INotificationService>());
        DataContext = ViewModel;
        ViewModel.LoadCommand.Execute(null);
    }
}
