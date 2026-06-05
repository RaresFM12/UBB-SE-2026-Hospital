using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class ProductCatalogueApiClient(HttpClient httpClient) : ApiClientBase(httpClient), IProductCatalogueService, IProductCatalogueApiClient
{
    private const string BaseUri = "api/product-catalogue";

    public async Task<IReadOnlyList<Item>> GetItemsAsync(
        string? search,
        IReadOnlyList<string>? categories = null,
        IReadOnlyList<(float Minimum, float Maximum)>? priceRanges = null,
        string? stockFilter = null,
        bool? discounted = null,
        IReadOnlyList<string>? substances = null,
        bool ascending = true,
        int page = 0,
        int pageSize = 30,
        string? sortBy = null,
        CancellationToken cancellationToken = default)
    {
        var query = new
        {
            search,
            categories,
            priceRanges = priceRanges?.Select(priceRange => new { minimum = priceRange.Minimum, maximum = priceRange.Maximum }).ToList(),
            stockFilter,
            discounted,
            substances,
            ascending,
            page,
            pageSize,
            sortBy,
        };

        return await PostAsync<object, List<Item>>($"{BaseUri}/search", query, cancellationToken) ?? [];
    }
}
