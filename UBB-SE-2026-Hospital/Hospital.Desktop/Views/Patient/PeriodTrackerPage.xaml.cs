using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Patient;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.Patient;

public sealed partial class PeriodTrackerPage : Page
{
    public PeriodTrackerViewModel ViewModel { get; private set; } = null!;

    public PeriodTrackerPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new PeriodTrackerViewModel(
            App.Services.GetRequiredService<IPeriodTrackerService>(),
            App.Services.GetRequiredService<ICurrentUserService>());
        DataContext = ViewModel;
        ViewModel.LoadCommand.Execute(null);
    }
}
