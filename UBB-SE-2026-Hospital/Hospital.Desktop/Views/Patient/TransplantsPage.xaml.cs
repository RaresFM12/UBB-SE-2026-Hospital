using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Patient;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.Patient;

public sealed partial class TransplantsPage : Page
{
    public TransplantViewModel ViewModel { get; private set; } = null!;

    public TransplantsPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new TransplantViewModel(App.Services.GetRequiredService<ITransplantService>());
        DataContext = ViewModel;
        ViewModel.LoadTransplantsCommand.Execute(null);
        ViewModel.FindMatchesCommand.Execute(null);
    }
}
