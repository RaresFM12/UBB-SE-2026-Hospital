using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IProductCatalogueService
{
    Task<IReadOnlyList<Item>> GetItemsAsync(
        string search,
        IReadOnlyList<string>? categories = null,
        IReadOnlyList<(float Minimum, float Maximum)>? priceRanges = null,
        string? stockFilter = null,
        bool? discounted = null,
        IReadOnlyList<string>? substances = null,
        bool ascending = true,
        int page = 0,
        int pageSize = 30,
        string? sortBy = null,
        CancellationToken cancellationToken = default);
}
