using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Doctor;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.Doctor;

public sealed partial class HangoutsPage : Page
{
    public HangoutsViewModel ViewModel { get; private set; } = null!;

    public HangoutsPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new HangoutsViewModel(App.Services.GetRequiredService<IHangoutService>());
        DataContext = ViewModel;
        ViewModel.LoadCommand.Execute(null);
    }
}
