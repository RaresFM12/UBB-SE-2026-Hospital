using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Hospital.Data.Models;
using Hospital.Desktop.Command;
using Hospital.Desktop.ViewModels.Base;
using Hospital.Shared.Proxies;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.Pharmacy;

/// <summary>
/// ViewModel for the Desktop Product Catalogue page.
/// Replicates the Web MVC ProductCatalogueController filter logic exactly.
/// </summary>
public class ProductCatalogueViewModel : ObservableObject
{
    private readonly IProductCatalogueService productCatalogueService;
    private readonly IBasketApiClient basketService;
    private readonly ICurrentUserService currentUserService;

    private const int ItemsPerPage = 12;
    private const int DefaultCartQuantity = 1;

    private const float FirstPriceRangeMinimum = 0f;
    private const float FirstPriceRangeMaximum = 49.99f;
    private const float SecondPriceRangeMinimum = 50f;
    private const float SecondPriceRangeMaximum = 99.99f;
    private const float ThirdPriceRangeMinimum = 100f;
    private const float ThirdPriceRangeMaximum = 199.99f;
    private const float FourthPriceRangeMinimum = 200f;
    private const float FourthPriceRangeMaximum = 499.99f;
    private const float FifthPriceRangeMinimum = 500f;

    private const string StockFilterAll = "all";
    private const string StockFilterInStock = "in_stock";
    private const string StockFilterLowStock = "low_stock";
    private const string DiscountFilterAll = "all";
    private const string DiscountFilterYes = "yes";
    private const string DiscountFilterNo = "no";
    private const string SortByDefault = "default";
    private const string SortByPrice = "price";
    private const string SortByNewest = "newest";

    private ObservableCollection<CatalogueItemViewModel> products = new();
    private string errorMessage = string.Empty;
    private bool isLoading;
    private string searchQuery = string.Empty;

    private int selectedSortByIndex;
    private int selectedSortDirectionIndex;

    private bool filterCategoryPainRelief;
    private bool filterCategoryWellness;
    private bool filterCategoryColdAndFlu;
    private bool filterCategoryAllergy;
    private bool filterCategoryDigestion;
    private bool filterCategorySkincare;
    private bool filterCategoryFirstAid;

    private bool filterPriceRange0To49;
    private bool filterPriceRange50To99;
    private bool filterPriceRange100To199;
    private bool filterPriceRange200To499;
    private bool filterPriceRange500Plus;

    private int selectedStockFilterIndex;
    private int selectedDiscountFilterIndex;

    private int currentPageIndex;
    private bool hasNextPage;

    // ── Properties ────────────────────────────────────────────────────────

    public ObservableCollection<CatalogueItemViewModel> Products
    {
        get => this.products;
        set => this.SetProperty(ref this.products, value);
    }

    public string ErrorMessage
    {
        get => this.errorMessage;
        set => this.SetProperty(ref this.errorMessage, value);
    }

    public bool IsLoading
    {
        get => this.isLoading;
        set => this.SetProperty(ref this.isLoading, value);
    }

    public string SearchQuery
    {
        get => this.searchQuery;
        set => this.SetProperty(ref this.searchQuery, value);
    }

    public int SelectedSortByIndex
    {
        get => this.selectedSortByIndex;
        set => this.SetProperty(ref this.selectedSortByIndex, value);
    }

    public int SelectedSortDirectionIndex
    {
        get => this.selectedSortDirectionIndex;
        set => this.SetProperty(ref this.selectedSortDirectionIndex, value);
    }

    // ── Category Filters ──────────────────────────────────────────────────

    public bool FilterCategoryPainRelief
    {
        get => this.filterCategoryPainRelief;
        set => this.SetProperty(ref this.filterCategoryPainRelief, value);
    }

    public bool FilterCategoryWellness
    {
        get => this.filterCategoryWellness;
        set => this.SetProperty(ref this.filterCategoryWellness, value);
    }

    public bool FilterCategoryColdAndFlu
    {
        get => this.filterCategoryColdAndFlu;
        set => this.SetProperty(ref this.filterCategoryColdAndFlu, value);
    }

    public bool FilterCategoryAllergy
    {
        get => this.filterCategoryAllergy;
        set => this.SetProperty(ref this.filterCategoryAllergy, value);
    }

    public bool FilterCategoryDigestion
    {
        get => this.filterCategoryDigestion;
        set => this.SetProperty(ref this.filterCategoryDigestion, value);
    }

    public bool FilterCategorySkincare
    {
        get => this.filterCategorySkincare;
        set => this.SetProperty(ref this.filterCategorySkincare, value);
    }

    public bool FilterCategoryFirstAid
    {
        get => this.filterCategoryFirstAid;
        set => this.SetProperty(ref this.filterCategoryFirstAid, value);
    }

    // ── Price Range Filters ───────────────────────────────────────────────

    public bool FilterPriceRange0To49
    {
        get => this.filterPriceRange0To49;
        set => this.SetProperty(ref this.filterPriceRange0To49, value);
    }

    public bool FilterPriceRange50To99
    {
        get => this.filterPriceRange50To99;
        set => this.SetProperty(ref this.filterPriceRange50To99, value);
    }

    public bool FilterPriceRange100To199
    {
        get => this.filterPriceRange100To199;
        set => this.SetProperty(ref this.filterPriceRange100To199, value);
    }

    public bool FilterPriceRange200To499
    {
        get => this.filterPriceRange200To499;
        set => this.SetProperty(ref this.filterPriceRange200To499, value);
    }

    public bool FilterPriceRange500Plus
    {
        get => this.filterPriceRange500Plus;
        set => this.SetProperty(ref this.filterPriceRange500Plus, value);
    }

    // ── Stock & Discount Filters ──────────────────────────────────────────

    public int SelectedStockFilterIndex
    {
        get => this.selectedStockFilterIndex;
        set => this.SetProperty(ref this.selectedStockFilterIndex, value);
    }

    public int SelectedDiscountFilterIndex
    {
        get => this.selectedDiscountFilterIndex;
        set => this.SetProperty(ref this.selectedDiscountFilterIndex, value);
    }

    // ── Pagination ────────────────────────────────────────────────────────

    public int CurrentPageIndex
    {
        get => this.currentPageIndex;
        set
        {
            this.SetProperty(ref this.currentPageIndex, value);
            this.RaisePropertyChanged(nameof(this.DisplayPageNumber));
            this.RaisePropertyChanged(nameof(this.HasPreviousPage));
        }
    }

    public int DisplayPageNumber => this.CurrentPageIndex + 1;
    public bool HasPreviousPage => this.CurrentPageIndex > 0;

    public bool HasNextPage
    {
        get => this.hasNextPage;
        set => this.SetProperty(ref this.hasNextPage, value);
    }

    // ── Commands ──────────────────────────────────────────────────────────

    public ICommand ApplyFiltersCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand AddToCartCommand { get; }

    // ── Constructor ───────────────────────────────────────────────────────

    public ProductCatalogueViewModel(
        IProductCatalogueService productCatalogueService,
        IBasketApiClient basketService,
        ICurrentUserService currentUserService)
    {
        this.productCatalogueService = productCatalogueService;
        this.basketService = basketService;
        this.currentUserService = currentUserService;

        this.ApplyFiltersCommand = new AsyncRelayCommand(this.ApplyFiltersAsync);
        this.PreviousPageCommand = new AsyncRelayCommand(this.GoToPreviousPageAsync, () => this.HasPreviousPage);
        this.NextPageCommand = new AsyncRelayCommand(this.GoToNextPageAsync, () => this.HasNextPage);
        this.AddToCartCommand = new RelayCommandWithOneParameter<CatalogueItemViewModel>(this.AddItemToCart);
    }

    // ── Public Methods ────────────────────────────────────────────────────

    public async Task LoadProductsAsync()
    {
        this.ErrorMessage = string.Empty;
        this.IsLoading = true;

        try
        {
            var searchString = string.IsNullOrWhiteSpace(this.SearchQuery) ? null : this.SearchQuery.Trim();

            var categories = this.BuildCategoryFilterList();
            var priceRanges = this.BuildPriceRangeFilterList();
            string? stockFilter = this.MapStockFilterIndex(this.SelectedStockFilterIndex);
            bool? discountFilter = this.MapDiscountFilterIndex(this.SelectedDiscountFilterIndex);
            string? sortBy = this.MapSortByIndex(this.SelectedSortByIndex);
            bool isAscending = this.SelectedSortDirectionIndex == 0;

            var fetchedItems = await this.productCatalogueService.GetItemsAsync(
                search: searchString,
                categories: categories.Count > 0 ? categories : null,
                priceRanges: priceRanges.Count > 0 ? priceRanges : null,
                stockFilter: stockFilter,
                discounted: discountFilter,
                substances: null,
                ascending: isAscending,
                page: this.CurrentPageIndex,
                pageSize: ItemsPerPage,
                sortBy: sortBy);

            // Peek next page for pagination
            var nextPageItems = await this.productCatalogueService.GetItemsAsync(
                search: searchString,
                categories: categories.Count > 0 ? categories : null,
                priceRanges: priceRanges.Count > 0 ? priceRanges : null,
                stockFilter: stockFilter,
                discounted: discountFilter,
                substances: null,
                ascending: isAscending,
                page: this.CurrentPageIndex + 1,
                pageSize: ItemsPerPage,
                sortBy: sortBy);

            this.HasNextPage = nextPageItems.Any();
            this.Products = new ObservableCollection<CatalogueItemViewModel>(
                fetchedItems.Select(item => new CatalogueItemViewModel(item)));
        }
        catch (UnauthorizedAccessException)
        {
            this.ErrorMessage = "Access denied. Please sign in again to continue.";
        }
        catch (Exception exception)
        {
            this.ErrorMessage = $"Failed to load products: {exception.Message}";
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    // ── Private Helpers ───────────────────────────────────────────────────

    private async Task ApplyFiltersAsync()
    {
        this.CurrentPageIndex = 0;
        await this.LoadProductsAsync();
    }

    private async Task GoToPreviousPageAsync()
    {
        if (this.HasPreviousPage)
        {
            this.CurrentPageIndex--;
            await this.LoadProductsAsync();
        }
    }

    private async Task GoToNextPageAsync()
    {
        if (this.HasNextPage)
        {
            this.CurrentPageIndex++;
            await this.LoadProductsAsync();
        }
    }

    private async void AddItemToCart(CatalogueItemViewModel catalogueItem)
    {
        if (catalogueItem == null || !catalogueItem.CanAddToCart)
        {
            return;
        }

        try
        {
            int userId = this.currentUserService.UserId;
            await this.basketService.AddToBasketAsync(userId, catalogueItem.ItemId, DefaultCartQuantity);
            System.Diagnostics.Debug.WriteLine($"Added '{catalogueItem.Name}' (ID={catalogueItem.ItemId}) to basket for user {userId}.");
        }
        catch (Exception exception)
        {
            this.ErrorMessage = $"Failed to add to cart: {exception.Message}";
        }
    }

    /// <summary>
    /// Builds the category filter list from the 7 checkbox states,
    /// matching the Web Index.cshtml hardcoded category values.
    /// </summary>
    private List<string> BuildCategoryFilterList()
    {
        var categories = new List<string>();
        if (this.FilterCategoryPainRelief) categories.Add("pain relief");
        if (this.FilterCategoryWellness) categories.Add("wellness");
        if (this.FilterCategoryColdAndFlu) categories.Add("cold and flu");
        if (this.FilterCategoryAllergy) categories.Add("allergy");
        if (this.FilterCategoryDigestion) categories.Add("digestion");
        if (this.FilterCategorySkincare) categories.Add("skincare");
        if (this.FilterCategoryFirstAid) categories.Add("first aid");
        return categories;
    }

    /// <summary>
    /// Builds the price range tuple list from the 5 checkbox states,
    /// matching the Web Controller price range constants exactly.
    /// </summary>
    private List<(float Minimum, float Maximum)> BuildPriceRangeFilterList()
    {
        var priceRanges = new List<(float, float)>();
        if (this.FilterPriceRange0To49) priceRanges.Add((FirstPriceRangeMinimum, FirstPriceRangeMaximum));
        if (this.FilterPriceRange50To99) priceRanges.Add((SecondPriceRangeMinimum, SecondPriceRangeMaximum));
        if (this.FilterPriceRange100To199) priceRanges.Add((ThirdPriceRangeMinimum, ThirdPriceRangeMaximum));
        if (this.FilterPriceRange200To499) priceRanges.Add((FourthPriceRangeMinimum, FourthPriceRangeMaximum));
        if (this.FilterPriceRange500Plus) priceRanges.Add((FifthPriceRangeMinimum, float.MaxValue));
        return priceRanges;
    }

    /// <summary>
    /// Maps the Stock Availability ComboBox index to the API filter string.
    /// Index 0 = "all" (null), 1 = "in_stock", 2 = "low_stock".
    /// </summary>
    private string? MapStockFilterIndex(int index)
    {
        return index switch
        {
            1 => IProductCatalogueService.StockFilterInStock,
            2 => IProductCatalogueService.StockFilterLowStock,
            _ => null,
        };
    }

    /// <summary>
    /// Maps the Discount Filter ComboBox index to the API boolean.
    /// Index 0 = "all" (null), 1 = true (discounted only), 2 = false (non-discounted).
    /// </summary>
    private bool? MapDiscountFilterIndex(int index)
    {
        return index switch
        {
            1 => true,
            2 => false,
            _ => null,
        };
    }

    /// <summary>
    /// Maps the Sort By ComboBox index to the API sort string.
    /// Index 0 = "default" (null), 1 = "price", 2 = "newest".
    /// </summary>
    private string? MapSortByIndex(int index)
    {
        return index switch
        {
            1 => IProductCatalogueService.SortByPrice,
            2 => IProductCatalogueService.SortByNewest,
            _ => null,
        };
    }
}
