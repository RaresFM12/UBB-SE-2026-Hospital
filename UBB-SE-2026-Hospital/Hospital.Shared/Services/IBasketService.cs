namespace Hospital.Shared.Services
{
    public interface IBasketService
    {
        void AddToBasket(int itemId, int quantity, float extraDiscountPercentage = 0f);
    }
}
