using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Patient;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.Patient;

public sealed partial class PatientsPage : Page
{
    public PatientViewModel ViewModel { get; private set; } = null!;

    public PatientsPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new PatientViewModel(App.Services.GetRequiredService<IPatientService>());
        DataContext = ViewModel;
    }
}
