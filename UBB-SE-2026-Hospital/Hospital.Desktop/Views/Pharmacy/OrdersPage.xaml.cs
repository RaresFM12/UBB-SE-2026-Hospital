using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Pharmacy;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.Pharmacy;

public sealed partial class OrdersPage : Page
{
    public OrdersViewModel ViewModel { get; private set; } = null!;

    public OrdersPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new OrdersViewModel(
            App.Services.GetRequiredService<IOrderService>(),
            App.Services.GetRequiredService<ICurrentUserService>());
        DataContext = ViewModel;
        ViewModel.LoadCommand.Execute(null);
    }
}
