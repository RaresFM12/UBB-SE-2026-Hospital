using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Patient;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.Patient;

public sealed partial class PrescriptionsPage : Page
{
    public PrescriptionViewModel ViewModel { get; private set; } = null!;

    public PrescriptionsPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new PrescriptionViewModel(App.Services.GetRequiredService<IPrescriptionService>());
        DataContext = ViewModel;
    }
}
