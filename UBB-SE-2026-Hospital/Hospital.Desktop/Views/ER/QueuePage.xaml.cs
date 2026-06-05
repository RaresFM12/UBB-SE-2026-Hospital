using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.ER;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.ER;

public sealed partial class QueuePage : Page
{
    public QueueViewModel ViewModel { get; private set; } = null!;

    public QueuePage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new QueueViewModel(
            App.Services.GetRequiredService<IERVisitService>(),
            App.Services.GetRequiredService<ITriageService>());
        DataContext = ViewModel;
        ViewModel.LoadQueueCommand.Execute(null);
    }
}
