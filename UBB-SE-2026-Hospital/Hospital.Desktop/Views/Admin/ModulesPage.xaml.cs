using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Admin;

namespace Hospital.Desktop.Views.Admin;

public sealed partial class ModulesPage : Page
{
    public ModulesViewModel ViewModel { get; private set; } = null!;

    public ModulesPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new ModulesViewModel();
        DataContext = ViewModel;
        ViewModel.LoadCommand.Execute(null);
    }
}
