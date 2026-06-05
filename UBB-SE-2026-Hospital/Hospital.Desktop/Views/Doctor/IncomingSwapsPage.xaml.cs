using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Doctor;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.Doctor;

public sealed partial class IncomingSwapsPage : Page
{
    public IncomingSwapsViewModel ViewModel { get; private set; } = null!;

    public IncomingSwapsPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new IncomingSwapsViewModel(
            App.Services.GetRequiredService<IShiftSwapService>(),
            App.Services.GetRequiredService<ICurrentUserService>());
        DataContext = ViewModel;
        ViewModel.LoadCommand.Execute(null);
    }
}
