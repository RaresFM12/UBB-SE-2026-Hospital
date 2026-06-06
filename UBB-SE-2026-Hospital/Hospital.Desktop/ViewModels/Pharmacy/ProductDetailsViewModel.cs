using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Hospital.Desktop.Command;
using Hospital.Desktop.ViewModels.Base;
using Hospital.Shared.Proxies;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.Pharmacy;

/// <summary>
/// ViewModel for the Product Details page (F4.5).
/// Handles quantity validation and Add to Basket with strict rules.
/// </summary>
public class ProductDetailsViewModel : ObservableObject
{
    private readonly IBasketApiClient basketService;
    private readonly ICurrentUserService currentUserService;

    private const int MinimumQuantity = 1;
    private const int MaximumAllowedQuantity = 50;
    private const int DefaultSelectedQuantity = 1;
    private const string InvalidQuantityMessage = "Invalid quantity selected";

    // ── Backing fields ────────────────────────────────────────────────────
    private CatalogueItemViewModel selectedProduct;
    private double selectedQuantity = DefaultSelectedQuantity;
    private string errorMessage = string.Empty;
    private string successMessage = string.Empty;
    private bool isAddingToCart;

    // ── Properties ────────────────────────────────────────────────────────

    public CatalogueItemViewModel SelectedProduct
    {
        get => this.selectedProduct;
        set => this.SetProperty(ref this.selectedProduct, value);
    }

    public double SelectedQuantity
    {
        get => this.selectedQuantity;
        set
        {
            this.SetProperty(ref this.selectedQuantity, value);
            this.ErrorMessage = string.Empty;
        }
    }

    public string ErrorMessage
    {
        get => this.errorMessage;
        set => this.SetProperty(ref this.errorMessage, value);
    }

    public string SuccessMessage
    {
        get => this.successMessage;
        set => this.SetProperty(ref this.successMessage, value);
    }

    public bool IsAddingToCart
    {
        get => this.isAddingToCart;
        set => this.SetProperty(ref this.isAddingToCart, value);
    }

    public bool CanAddToCart => this.SelectedProduct != null && this.SelectedProduct.CanAddToCart;

    // ── Commands ──────────────────────────────────────────────────────────

    public ICommand AddToBasketCommand { get; }

    // ── Constructor ───────────────────────────────────────────────────────

    public ProductDetailsViewModel(
        IBasketApiClient basketService,
        ICurrentUserService currentUserService)
    {
        this.basketService = basketService;
        this.currentUserService = currentUserService;
        this.AddToBasketCommand = new AsyncRelayCommand(this.ExecuteAddToBasketAsync, () => this.CanAddToCart && !this.IsAddingToCart);
    }

    // ── Public Methods ────────────────────────────────────────────────────

    public void LoadProduct(CatalogueItemViewModel product)
    {
        this.SelectedProduct = product;
        this.SelectedQuantity = DefaultSelectedQuantity;
        this.ErrorMessage = string.Empty;
        this.SuccessMessage = string.Empty;
        this.RaisePropertyChanged(nameof(this.CanAddToCart));
    }

    // ── Private Methods ───────────────────────────────────────────────────

    private async Task ExecuteAddToBasketAsync()
    {
        this.ErrorMessage = string.Empty;
        this.SuccessMessage = string.Empty;

        if (!this.ValidateQuantity())
        {
            this.ErrorMessage = InvalidQuantityMessage;
            return;
        }

        this.IsAddingToCart = true;

        try
        {
            int quantityToAdd = (int)this.SelectedQuantity;
            int userId = this.currentUserService.UserId;
            await this.basketService.AddToBasketAsync(userId, this.SelectedProduct.ItemId, quantityToAdd);
            this.SuccessMessage = $"Added {quantityToAdd}x \"{this.SelectedProduct.Name}\" to your basket.";
        }
        catch (Exception exception)
        {
            this.ErrorMessage = $"Failed to add to basket: {exception.Message}";
        }
        finally
        {
            this.IsAddingToCart = false;
        }
    }

    /// <summary>
    /// Validates the selected quantity against the strict rules:
    /// - Must be a natural number > 0
    /// - Must be ≤ 50
    /// - Must be ≤ available stock
    /// </summary>
    private bool ValidateQuantity()
    {
        int quantity = (int)this.SelectedQuantity;

        if (quantity < MinimumQuantity)
        {
            return false;
        }

        if (quantity > MaximumAllowedQuantity)
        {
            return false;
        }

        if (this.SelectedProduct == null || quantity > this.SelectedProduct.Quantity)
        {
            return false;
        }

        if (Math.Abs(this.SelectedQuantity - quantity) > 0.001)
        {
            return false;
        }

        return true;
    }
}
