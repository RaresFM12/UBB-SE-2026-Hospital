using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Admin;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.Admin;

public sealed partial class SubstancesPage : Page
{
    public SubstancesViewModel ViewModel { get; private set; } = null!;

    public SubstancesPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new SubstancesViewModel(App.Services.GetRequiredService<IAdminService>());
        DataContext = ViewModel;
        ViewModel.LoadCommand.Execute(null);
    }
}
