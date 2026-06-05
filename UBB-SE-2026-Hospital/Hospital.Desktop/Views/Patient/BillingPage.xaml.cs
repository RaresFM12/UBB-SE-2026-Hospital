using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Patient;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.Patient;

public sealed partial class BillingPage : Page
{
    public BillingViewModel ViewModel { get; private set; } = null!;

    public BillingPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new BillingViewModel(App.Services.GetRequiredService<IBillingService>());
        DataContext = ViewModel;
    }
}
