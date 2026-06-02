namespace Hospital.Shared.Services;

public interface IBasketService
{
    Task AddToBasketAsync(int userId, int itemId, int quantity, float extraDiscountPercentage = 0f, CancellationToken cancellationToken = default);
}
