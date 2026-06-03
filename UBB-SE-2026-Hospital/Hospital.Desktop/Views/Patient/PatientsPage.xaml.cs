using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Patient;
using Microsoft.UI.Xaml.Input;

namespace Hospital.Desktop.Views.Patient;

public sealed partial class PatientsPage : Page
{
    public PatientViewModel ViewModel { get; private set; } = null!;
    private PatientProfileWindow? patientProfileWindow;
    private bool isOpeningPatientProfile;

    public PatientsPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = App.Services.GetRequiredService<PatientViewModel>();
        DataContext = ViewModel;
        ViewModel.LoadPatientsCommand.Execute(null);
    }

    private async void PatientListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (ViewModel.SelectedPatient is null || isOpeningPatientProfile)
        {
            return;
        }

        try
        {
            isOpeningPatientProfile = true;

            if (patientProfileWindow is not null)
            {
                await patientProfileWindow.InitializeAsync(ViewModel.SelectedPatient.PatientId);
                patientProfileWindow.Activate();
                return;
            }

            PatientProfileWindow window = App.Services.GetRequiredService<PatientProfileWindow>();
            window.Closed += (_, _) => patientProfileWindow = null;
            await window.InitializeAsync(ViewModel.SelectedPatient.PatientId);

            patientProfileWindow = window;
            window.Activate();
        }
        finally
        {
            isOpeningPatientProfile = false;
        }
    }
}
