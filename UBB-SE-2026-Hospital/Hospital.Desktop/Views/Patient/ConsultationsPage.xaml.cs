using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Patient;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.Patient;

public sealed partial class ConsultationsPage : Page
{
    public ConsultationsViewModel ViewModel { get; private set; } = null!;

    public ConsultationsPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new ConsultationsViewModel(
            App.Services.GetRequiredService<IPatientService>(),
            App.Services.GetRequiredService<IBillingService>());
        DataContext = ViewModel;
    }
}
