using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.ER;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.ER;

public sealed partial class TriagePage : Page
{
    public TriageViewModel ViewModel { get; private set; } = null!;

    public TriagePage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new TriageViewModel(App.Services.GetRequiredService<ITriageService>());
        DataContext = ViewModel;
    }
}
