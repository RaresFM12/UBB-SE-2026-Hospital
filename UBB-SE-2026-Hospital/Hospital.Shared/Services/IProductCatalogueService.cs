using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IProductCatalogueService
{
    const string StockFilterInStock = "in_stock";
    const string StockFilterLowStock = "low_stock";
    const string SortByPrice = "price";
    const string SortByNewest = "newest";
    const int DefaultPageSize = 30;
    const int LowStockThreshold = 10;

    Task<IReadOnlyList<Item>> GetItemsAsync(
        string? search,
        IReadOnlyList<string>? categories = null,
        IReadOnlyList<(float Minimum, float Maximum)>? priceRanges = null,
        string? stockFilter = null,
        bool? discounted = null,
        IReadOnlyList<string>? substances = null,
        bool ascending = true,
        int page = 0,
        int pageSize = DefaultPageSize,
        string? sortBy = null,
        CancellationToken cancellationToken = default);
}
