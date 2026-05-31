using Hospital.Shared.Models.StaffPharmacy;

namespace Hospital.Shared.Services;

public interface IBasketService
{
    Task<Dictionary<int, BasketEntry>> GetBasketAsync(int userId, CancellationToken cancellationToken = default);

    Task SaveBasketAsync(int userId, Dictionary<int, BasketEntry> basket, CancellationToken cancellationToken = default);

    Task ClearBasketAsync(int userId, CancellationToken cancellationToken = default);

    Task AddToBasketAsync(int userId, int itemId, int quantity, float extraDiscountPercentage = 0f, CancellationToken cancellationToken = default);
}
