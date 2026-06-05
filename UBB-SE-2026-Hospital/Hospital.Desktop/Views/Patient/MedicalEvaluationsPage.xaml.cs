using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Patient;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.Patient;

public sealed partial class MedicalEvaluationsPage : Page
{
    public MedicalEvaluationsViewModel ViewModel { get; private set; } = null!;

    public MedicalEvaluationsPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new MedicalEvaluationsViewModel(App.Services.GetRequiredService<IMedicalEvaluationService>());
        DataContext = ViewModel;
        ViewModel.LoadCommand.Execute(null);
    }
}
