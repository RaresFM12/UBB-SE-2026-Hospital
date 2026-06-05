using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Doctor;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.Doctor;

public sealed partial class ShiftSwapRequestsPage : Page
{
    public ShiftSwapRequestsViewModel ViewModel { get; private set; } = null!;

    public ShiftSwapRequestsPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new ShiftSwapRequestsViewModel(
            App.Services.GetRequiredService<IShiftSwapService>(),
            App.Services.GetRequiredService<ICurrentUserService>());
        DataContext = ViewModel;
        ViewModel.LoadCommand.Execute(null);
    }
}
