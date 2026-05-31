using Hospital.Shared.Models.StaffPharmacy;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class BasketService : IBasketService
{
    public Task<Dictionary<int, BasketEntry>> GetBasketAsync(int userId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task SaveBasketAsync(int userId, Dictionary<int, BasketEntry> basket, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task ClearBasketAsync(int userId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task AddToBasketAsync(int userId, int itemId, int quantity, float extraDiscountPercentage = 0f, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
