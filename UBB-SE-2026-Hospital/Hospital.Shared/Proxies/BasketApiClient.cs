using Hospital.Shared.Services;
using SharedBasketEntry = Hospital.Data.Models.BasketEntryDto;

namespace Hospital.Shared.Proxies;

public class BasketApiClient(HttpClient httpClient) : ApiClientBase(httpClient), IBasketService, IBasketApiClient
{
    private const string BaseUri = "api/baskets";

    public async Task<Dictionary<int, SharedBasketEntry>> GetBasketAsync(int userId, CancellationToken cancellationToken = default)
        => await GetAsync<Dictionary<int, SharedBasketEntry>>($"{BaseUri}/{userId}", cancellationToken) ?? [];

    public async Task SaveBasketAsync(int userId, Dictionary<int, SharedBasketEntry> basket, CancellationToken cancellationToken = default)
        => await PutAsync($"{BaseUri}/{userId}", basket ?? [], cancellationToken);

    public async Task ClearBasketAsync(int userId, CancellationToken cancellationToken = default)
        => await DeleteAsync($"{BaseUri}/{userId}", cancellationToken);

    public async Task AddToBasketAsync(int userId, int itemId, int quantity, float extraDiscountPercentage = 0f, CancellationToken cancellationToken = default)
    {
        Dictionary<int, SharedBasketEntry> basket = await GetBasketAsync(userId, cancellationToken);
        if (basket.TryGetValue(itemId, out SharedBasketEntry? existing) && existing is not null)
        {
            existing.Quantity += quantity;
            existing.ExtraDiscountPercentage = extraDiscountPercentage;
        }
        else
        {
            basket[itemId] = new SharedBasketEntry
            {
                ItemId = itemId,
                Quantity = quantity,
                ExtraDiscountPercentage = extraDiscountPercentage,
            };
        }

        await SaveBasketAsync(userId, basket, cancellationToken);
    }
}
