using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.ER;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.ER;

public sealed partial class RoomManagementPage : Page
{
    public RoomManagementViewModel ViewModel { get; private set; } = null!;

    public RoomManagementPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new RoomManagementViewModel(App.Services.GetRequiredService<IERRoomService>());
        DataContext = ViewModel;
    }
}
