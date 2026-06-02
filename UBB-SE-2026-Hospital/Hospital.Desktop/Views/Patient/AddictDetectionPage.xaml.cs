using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Patient;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.Patient;

public sealed partial class AddictDetectionPage : Page
{
    public AddictDetectionViewModel ViewModel { get; private set; } = null!;

    public AddictDetectionPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new AddictDetectionViewModel(App.Services.GetRequiredService<IAddictDetectionService>());
        DataContext = ViewModel;
        ViewModel.LoadCandidatesCommand.Execute(null);
    }
}
