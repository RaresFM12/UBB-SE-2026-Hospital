using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.ER;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.ER;

public sealed partial class PatientRegistrationPage : Page
{
    public PatientRegistrationViewModel ViewModel { get; private set; } = null!;

    public PatientRegistrationPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new PatientRegistrationViewModel(
            App.Services.GetRequiredService<IPatientService>(),
            App.Services.GetRequiredService<IERVisitService>());
        DataContext = ViewModel;
    }
}
