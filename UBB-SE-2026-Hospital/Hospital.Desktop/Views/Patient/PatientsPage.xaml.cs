using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Patient;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml;

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

    // ── Tab switching ─────────────────────────────────────────
    private void ShowActivePatients_Click(object sender, RoutedEventArgs e)
        => ViewModel.ShowDeceasedArchive = false;

    private void ShowArchive_Click(object sender, RoutedEventArgs e)
        => ViewModel.ShowDeceasedArchive = true;

    // ── Active patient actions ────────────────────────────────
    private async void UpdatePatient_Click(object sender, RoutedEventArgs e)
        => await ViewModel.UpdateSelectedPatientAsync();

    private async void ArchivePatient_Click(object sender, RoutedEventArgs e)
        => await ViewModel.ArchiveSelectedPatientAsync();

    // ── Archive patient actions ───────────────────────────────
    private async void Dearchive_Click(object sender, RoutedEventArgs e)
        => await ViewModel.DearchiveSelectedPatientAsync();

    private async void MarkAsDeceased_Click(object sender, RoutedEventArgs e)
        => await ViewModel.MarkSelectedPatientAsDeceasedAsync();

    private async void AddDonor_Click(object sender, RoutedEventArgs e)
        => await ViewModel.SetDonorStatusAsync(true);

    private async void RemoveDonor_Click(object sender, RoutedEventArgs e)
        => await ViewModel.SetDonorStatusAsync(false);

    // ── Double-tap opens patient profile ─────────────────────
    private async void PatientListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (ViewModel.SelectedPatient is null || isOpeningPatientProfile)
            return;

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
