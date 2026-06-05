using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Patient;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.Patient;

public sealed partial class StatisticsPage : Page
{
    public StatisticsViewModel ViewModel { get; private set; } = null!;

    public StatisticsPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new StatisticsViewModel(App.Services.GetRequiredService<IStatisticsService>());
        DataContext = ViewModel;
        ViewModel.LoadStatisticsCommand.Execute(null);
    }
}
