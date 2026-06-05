using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Admin;
using Hospital.Shared.Proxies;

namespace Hospital.Desktop.Views.Admin;

public sealed partial class GhostPage : Page
{
    public GhostViewModel ViewModel { get; private set; } = null!;

    public GhostPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new GhostViewModel(App.Services.GetRequiredService<IGhostApiClient>());
        DataContext = ViewModel;
        ViewModel.LoadCommand.Execute(null);
    }
}
