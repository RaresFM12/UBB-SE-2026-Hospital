namespace Hospital.Shared.Services
{
    public class BasketService : IBasketService
    {
        private readonly IOrderService orderService;

        public BasketService(IOrderService orderService)
        {
            this.orderService = orderService;
        }

        public void AddToBasket(int itemId, int quantity, float extraDiscountPercentage = 0f)
        {
            this.orderService.AddItemToBasket(itemId, quantity, extraDiscountPercentage);
        }
    }
}
