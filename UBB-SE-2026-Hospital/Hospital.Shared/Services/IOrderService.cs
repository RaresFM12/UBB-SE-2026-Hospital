using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IOrderService
{
    Task<IReadOnlyList<Order>> GetAllOrdersAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> GetOrdersByClientAsync(int clientId, CancellationToken cancellationToken = default);

    Task<Order?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default);

    Task<bool> OrderExistsAsync(int orderId, CancellationToken cancellationToken = default);

    Task<int> CreateOrderAsync(int clientId, DateOnly pickUpDate, bool isCompleted, bool isExpired, CancellationToken cancellationToken = default);

    Task UpdateOrderAsync(Order order, CancellationToken cancellationToken = default);

    Task DeleteOrderAsync(int orderId, CancellationToken cancellationToken = default);

    Task PlaceOrderFromBasketAsync(int userId, DateOnly chosenPickUpDate, CancellationToken cancellationToken = default);

    Task CompleteOrderAsync(int orderId, Dictionary<int, (int Quantity, float Discount)> updatedQuantities, CancellationToken cancellationToken = default);

    Task CancelOrderAsync(int orderId, CancellationToken cancellationToken = default);

    Task ExpireOverdueOrdersAsync(CancellationToken cancellationToken = default);
}
