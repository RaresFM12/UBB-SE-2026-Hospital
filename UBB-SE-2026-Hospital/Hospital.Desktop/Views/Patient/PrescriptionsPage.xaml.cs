using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Patient;

namespace Hospital.Desktop.Views.Patient;

public sealed partial class PrescriptionsPage : Page
{
    public PrescriptionViewModel ViewModel { get; private set; } = null!;

    public PrescriptionsPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = App.Services.GetRequiredService<PrescriptionViewModel>();
        DataContext = ViewModel;
        ViewModel.LoadPrescriptionsCommand.Execute(null);
    }
}
