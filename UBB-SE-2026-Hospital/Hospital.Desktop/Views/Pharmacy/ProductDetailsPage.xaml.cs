using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Hospital.Desktop.ViewModels.Pharmacy;

namespace Hospital.Desktop.Views.Pharmacy;

public sealed partial class ProductDetailsPage : Page
{
    public ProductDetailsViewModel ViewModel { get; }

    public ProductDetailsPage()
    {
        this.InitializeComponent();
        this.ViewModel = App.Services.GetRequiredService<ProductDetailsViewModel>();
        this.DataContext = this.ViewModel;
    }

    protected override void OnNavigatedTo(NavigationEventArgs navigateArgs)
    {
        base.OnNavigatedTo(navigateArgs);

        if (navigateArgs.Parameter is CatalogueItemViewModel catalogueItem)
        {
            this.ViewModel.LoadProduct(catalogueItem);
        }
    }

    private void BackButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs eventArgs)
    {
        if (this.Frame.CanGoBack)
        {
            this.Frame.GoBack();
        }
    }
}
