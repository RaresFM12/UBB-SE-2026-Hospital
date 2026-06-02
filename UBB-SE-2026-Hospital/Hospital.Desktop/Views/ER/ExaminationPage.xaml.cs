using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.ER;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.ER;

public sealed partial class ExaminationPage : Page
{
    public ExaminationViewModel ViewModel { get; private set; } = null!;

    public ExaminationPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new ExaminationViewModel(
            App.Services.GetRequiredService<IExaminationService>(),
            App.Services.GetRequiredService<IERVisitService>(),
            App.Services.GetRequiredService<IERRoomService>(),
            App.Services.GetRequiredService<ITriageService>());
        DataContext = ViewModel;
    }
}
