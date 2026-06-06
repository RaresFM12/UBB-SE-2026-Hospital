using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Hospital.Desktop.ViewModels.Pharmacy;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Views.Pharmacy;

public sealed partial class BasketPage : Page
{
    public BasketViewModel ViewModel { get; private set; } = null!;

    public BasketPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = new BasketViewModel(
            App.Services.GetRequiredService<IOrderService>(),
            App.Services.GetRequiredService<ICurrentUserService>());
        DataContext = ViewModel;
        ViewModel.LoadCommand.Execute(null);
    }

    private async void Remove_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Hospital.Data.Models.BasketItemViewModel item)
        {
            await ViewModel.RemoveCommand.ExecuteAsync(item);
        }
    }
}
