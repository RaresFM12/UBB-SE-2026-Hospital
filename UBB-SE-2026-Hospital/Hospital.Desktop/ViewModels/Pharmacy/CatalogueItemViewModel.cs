using System;
using System.Linq;
using Hospital.Data.Models;
using Hospital.Desktop.Converters;

namespace Hospital.Desktop.ViewModels.Pharmacy;

/// <summary>
/// Desktop wrapper around <see cref="Item"/> that replicates the Web MVC
/// CatalogueItemViewModel logic for discount calculation, stock status, etc.
/// </summary>
public class CatalogueItemViewModel
{
    private const float PercentageDivisor = 100f;
    private const int LowStockThreshold = 10;
    private const int OutOfStockQuantity = 0;
    private const string SubstanceSeparator = ", ";
    private const string NoSubstancesText = "None";
    private const string DesktopImagePrefix = "ms-appx:///";

    public int ItemId { get; }
    public string Name { get; }
    public string Producer { get; }
    public string Category { get; }
    public string Label { get; }
    public int NumberOfPills { get; }
    public string SubstancesText { get; }
    public float OldPrice { get; }
    public float DiscountPercentage { get; }
    public float FinalPrice { get; }
    public int Quantity { get; }
    public string ImagePath { get; }
    public string Description { get; }

    public bool HasDiscount => this.DiscountPercentage > 0;

    public string StockText => this.Quantity == OutOfStockQuantity
        ? "Out of stock"
        : this.Quantity < LowStockThreshold
            ? $"Only {this.Quantity} in stock"
            : "In stock";

    public StockLevel StockLevel => this.Quantity == OutOfStockQuantity
        ? StockLevel.OutOfStock
        : this.Quantity < LowStockThreshold
            ? StockLevel.LowStock
            : StockLevel.InStock;

    public bool CanAddToCart => this.Quantity > OutOfStockQuantity;

    public string OldPriceFormatted => $"{this.OldPrice:F2} lei";
    public string FinalPriceFormatted => $"{this.FinalPrice:F2} lei";
    public string DiscountBadgeText => $"-{Math.Round(this.DiscountPercentage, 2)}%";

    public CatalogueItemViewModel(Item item)
    {
        this.ItemId = item.Id;
        this.Name = item.Name;
        this.Producer = item.Producer;
        this.Category = item.Category;
        this.Label = item.Label;
        this.NumberOfPills = item.NumberOfPills;
        this.OldPrice = item.Price;
        this.DiscountPercentage = item.DiscountPercentage;
        this.FinalPrice = this.OldPrice * (1 - (this.DiscountPercentage / PercentageDivisor));
        this.Quantity = item.Quantity;
        this.Description = item.Description;

        string cleanImage = item.ImagePath?.TrimStart('/') ?? string.Empty;
        if (cleanImage.StartsWith(DesktopImagePrefix))
        {
            cleanImage = cleanImage.Replace(DesktopImagePrefix, string.Empty);
        }
        this.ImagePath = cleanImage;

        this.SubstancesText = item.ActiveSubstances != null && item.ActiveSubstances.Any()
            ? string.Join(SubstanceSeparator, item.ActiveSubstances.Select(substance => $"{substance.Key} ({substance.Value})"))
            : NoSubstancesText;
    }
}
