using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Patient;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.Patient;

public sealed partial class OrganDonorPage : Page
{
    public OrganDonorViewModel ViewModel { get; private set; } = null!;

    public OrganDonorPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new OrganDonorViewModel(
            App.Services.GetRequiredService<IBloodCompatibilityService>(),
            App.Services.GetRequiredService<ITransplantService>());
        DataContext = ViewModel;
    }
}
