using Hospital.Desktop.ViewModels.Patient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Hospital.Desktop.Views.Patient;

public sealed partial class PeriodTrackerPage : Page
{
    public PeriodTrackerViewModel ViewModel { get; }

    public PeriodTrackerPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<PeriodTrackerViewModel>();
        DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadCommand.ExecuteAsync(null);
    }
}
