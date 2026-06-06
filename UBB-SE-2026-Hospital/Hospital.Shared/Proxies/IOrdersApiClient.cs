using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies
{
    public interface IOrdersApiClient
    {
        OrderItemRepositoryFacade ItemsRepository { get; }
        OrderRepositoryFacade OrdersRepository { get; }
        OrderUserRepositoryFacade UsersRepository { get; }

        Task AddItemToBasketAsync(int userId, int itemId, int quantity, float extraDiscountPercentage = 0, CancellationToken cancellationToken = default);
        Task ApplyPrescriptionToBasketAsync(int userId, string prescriptionId, CancellationToken cancellationToken = default);
        Tuple<float, float> CalculateBasketTotalSum(List<BasketItemViewModel> basketItems);
        void CancelOrder(int orderId);
        Task CancelOrderAsync(int orderId, CancellationToken cancellationToken = default);
        void CompleteOrder(int orderId, Dictionary<int, Tuple<int, float>> updatedItems);
        Task CompleteOrderAsync(int orderId, Dictionary<int, (int Quantity, float Discount)> updatedQuantities, CancellationToken cancellationToken = default);
        Task<int> CreateOrderAsync(int clientId, DateOnly pickUpDate, bool isCompleted, bool isExpired, CancellationToken cancellationToken = default);
        Task DeleteOrderAsync(int orderId, CancellationToken cancellationToken = default);
        void ExpireOverdueOrders();
        Task ExpireOverdueOrdersAsync(CancellationToken cancellationToken = default);
        List<Order> GetAllOrders();
        Task<IReadOnlyList<Order>> GetAllOrdersAsync(CancellationToken cancellationToken = default);
        Task<List<BasketItemViewModel>> GetBasketItemsAsync(int userId, CancellationToken cancellationToken = default);
        Order? GetOrder(int orderId);
        Task<Order?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Order>> GetOrdersByClientAsync(int clientId, CancellationToken cancellationToken = default);
        List<Order> GetOrdersOfClient(int clientId);
        void ModifyIncompleteOrder(int orderId, Dictionary<int, Tuple<int, float>> updatedItems, DateOnly pickUpDate);
        Task<bool> OrderExistsAsync(int orderId, CancellationToken cancellationToken = default);
        Task PlaceOrderFromBasketAsync(int userId, DateOnly chosenPickUpDate, CancellationToken cancellationToken = default);
        Task RemoveFromBasketAsync(int userId, int itemId, CancellationToken cancellationToken = default);
        void ResubmitExpiredOrder(int orderId, DateOnly pickUpDate);
        Task UpdateBasketItemQuantityAsync(int userId, int itemId, int quantity, CancellationToken cancellationToken = default);
        Task UpdateOrderAsync(Order order, CancellationToken cancellationToken = default);
    }
}