using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.ER;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.ER;

public sealed partial class TransferLogPage : Page
{
    public TransferLogViewModel ViewModel { get; private set; } = null!;

    public TransferLogPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new TransferLogViewModel(
            App.Services.GetRequiredService<ITransferLogService>(),
            App.Services.GetRequiredService<IERVisitService>());
        ViewModel.XamlRoot = XamlRoot;
        DataContext = ViewModel;
        ViewModel.LoadDataCommand.Execute(null);
    }
}
