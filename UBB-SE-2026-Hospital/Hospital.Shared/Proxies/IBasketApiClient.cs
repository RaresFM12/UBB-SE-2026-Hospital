using SharedBasketEntry = Hospital.Data.Models.BasketEntryDto;

namespace Hospital.Shared.Proxies;

public interface IBasketApiClient
{
    Task AddToBasketAsync(int userId, int itemId, int quantity, float extraDiscountPercentage = 0f, CancellationToken cancellationToken = default);
    Task SaveBasketAsync(int userId, Dictionary<int, SharedBasketEntry> basket, CancellationToken cancellationToken = default);
    Task<Dictionary<int, SharedBasketEntry>> GetBasketAsync(int userId, CancellationToken cancellationToken = default);
    Task ClearBasketAsync(int userId, CancellationToken cancellationToken = default);
}
