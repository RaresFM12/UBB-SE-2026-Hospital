using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class OrderService : IOrderService
{
    public Task<IReadOnlyList<Order>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<Order>> GetOrdersByClientAsync(int clientId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<Order?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> OrderExistsAsync(int orderId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<int> CreateOrderAsync(int clientId, DateOnly pickUpDate, bool isCompleted, bool isExpired, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task UpdateOrderAsync(Order order, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task DeleteOrderAsync(int orderId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task PlaceOrderFromBasketAsync(int userId, DateOnly chosenPickUpDate, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task CompleteOrderAsync(int orderId, Dictionary<int, (int Quantity, float Discount)> updatedQuantities, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task CancelOrderAsync(int orderId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task ExpireOverdueOrdersAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
