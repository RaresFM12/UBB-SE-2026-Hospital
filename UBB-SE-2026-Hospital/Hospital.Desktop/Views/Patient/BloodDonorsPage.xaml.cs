using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Patient;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.Patient;

public sealed partial class BloodDonorsPage : Page
{
    public BloodDonorsViewModel ViewModel { get; private set; } = null!;

    public BloodDonorsPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new BloodDonorsViewModel(
            App.Services.GetRequiredService<IBloodCompatibilityService>(),
            App.Services.GetRequiredService<IPatientService>());
        DataContext = ViewModel;
        ViewModel.LoadPatientsCommand.Execute(null);
    }
}
