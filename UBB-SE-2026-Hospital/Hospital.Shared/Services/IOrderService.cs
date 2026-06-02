using Hospital.Data.Models;
using Hospital.Shared.Models.Orders;

namespace Hospital.Shared.Services;

public interface IOrderService
{
    User ActiveUser { get; }
    OrderRepositoryFacade OrdersRepository { get; }
    OrderItemRepositoryFacade ItemsRepository { get; }
    OrderUserRepositoryFacade UsersRepository { get; }
    Task<IReadOnlyList<Order>> GetAllOrdersAsync(CancellationToken cancellationToken = default);
    List<Order> GetAllOrders();
    Task<IReadOnlyList<Order>> GetOrdersByClientAsync(int clientId, CancellationToken cancellationToken = default);
    List<Order> GetOrdersOfClient(int clientId);
    Task<Order?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default);
    Order? GetOrder(int orderId);
    Task<bool> OrderExistsAsync(int orderId, CancellationToken cancellationToken = default);
    Task<int> CreateOrderAsync(int clientId, DateOnly pickUpDate, bool isCompleted, bool isExpired, CancellationToken cancellationToken = default);
    Task UpdateOrderAsync(Order order, CancellationToken cancellationToken = default);
    void ModifyIncompleteOrder(int orderId, Dictionary<int, Tuple<int, float>> updatedItems, DateOnly pickUpDate);
    Task DeleteOrderAsync(int orderId, CancellationToken cancellationToken = default);
    Task PlaceOrderFromBasketAsync(int userId, DateOnly chosenPickUpDate, CancellationToken cancellationToken = default);
    void PlaceOrderFromBasket(DateOnly chosenPickUpDate);
    Task CompleteOrderAsync(int orderId, Dictionary<int, (int Quantity, float Discount)> updatedQuantities, CancellationToken cancellationToken = default);
    void CompleteOrder(int orderId, Dictionary<int, Tuple<int, float>> updatedItems);
    Task CancelOrderAsync(int orderId, CancellationToken cancellationToken = default);
    void CancelOrder(int orderId);
    Task ExpireOverdueOrdersAsync(CancellationToken cancellationToken = default);
    void ExpireOverdueOrders();
    void ResubmitExpiredOrder(int orderId, DateOnly pickUpDate);
    List<BasketItemViewModel> GetBasketItems();
    Tuple<float, float> CalculateBasketTotalSum(List<BasketItemViewModel> basketItems);
    void AddItemToBasket(int itemId, int quantity, float extraDiscountPercentage = 0f);
    void AddToBasket(int itemId, int quantity);
    void UpdateBasketItemQuantity(int itemId, int quantity);
    void RemoveFromBasket(int itemId);
    void ApplyPrescriptionToBasket(string prescriptionId);
}

public sealed class OrderRepositoryFacade(IOrderService orderService)
{
    public List<Order> GetOrdersOfClient(int clientId)
    {
        return orderService.GetOrdersOfClient(clientId);
    }

    public List<Order> GetAllOrders()
    {
        return orderService.GetAllOrders();
    }

    public Order? GetOrder(int orderId)
    {
        return orderService.GetOrder(orderId);
    }
}

public sealed class OrderItemRepositoryFacade(Func<int, Item?> getItemById)
{
    public Item? GetItemById(int itemId)
    {
        return getItemById(itemId);
    }
}

public sealed class OrderUserRepositoryFacade(Func<List<User>> getAllUsers, Func<int, User?> getUserById)
{
    public List<User> GetAllUsers()
    {
        return getAllUsers();
    }

    public User? GetUserById(int userId)
    {
        return getUserById(userId);
    }
}
