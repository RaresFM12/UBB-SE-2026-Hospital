using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.ER;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.ER;

public sealed partial class RoomAssignmentPage : Page
{
    public RoomAssignmentViewModel ViewModel { get; private set; } = null!;

    public RoomAssignmentPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new RoomAssignmentViewModel(
            App.Services.GetRequiredService<IERVisitService>(),
            App.Services.GetRequiredService<IERRoomService>(),
            App.Services.GetRequiredService<ITriageService>(),
            App.Services.GetRequiredService<IPatientService>());
        ViewModel.XamlRoot = XamlRoot;
        DataContext = ViewModel;
        ViewModel.LoadDataCommand.Execute(null);
    }
}
