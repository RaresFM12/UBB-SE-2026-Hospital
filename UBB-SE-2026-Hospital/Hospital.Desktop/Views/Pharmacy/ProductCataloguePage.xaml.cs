using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Hospital.Desktop.ViewModels.Pharmacy;

namespace Hospital.Desktop.Views.Pharmacy;

public sealed partial class ProductCataloguePage : Page
{
    public ProductCatalogueViewModel ViewModel { get; }

    public ProductCataloguePage()
    {
        this.InitializeComponent();
        this.ViewModel = App.Services.GetRequiredService<ProductCatalogueViewModel>();
        this.DataContext = this.ViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs navigateArgs)
    {
        base.OnNavigatedTo(navigateArgs);
        await this.ViewModel.LoadProductsAsync();
    }

    private async void ApplyFiltersButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs eventArgs)
    {
        this.ViewModel.CurrentPageIndex = 0;
        await this.ViewModel.LoadProductsAsync();
    }

    private void ProductCard_ItemClick(object sender, ItemClickEventArgs clickArgs)
    {
        if (clickArgs.ClickedItem is CatalogueItemViewModel selectedItem)
        {
            this.Frame.Navigate(typeof(ProductDetailsPage), selectedItem);
        }
    }
}
