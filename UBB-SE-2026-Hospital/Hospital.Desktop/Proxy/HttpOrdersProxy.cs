using Hospital.Data.Models;
using Hospital.Shared.Models.Orders;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Proxy;

public class HttpOrdersProxy(HttpClient httpClient) : ProxyBase(httpClient), IOrderService
{
    private const string BaseUri = "api/orders";
    private const string BasketUri = "api/basket";

    public OrderRepositoryFacade OrdersRepository => new(this);
    public OrderItemRepositoryFacade ItemsRepository => new(itemId => Task.Run(async () => await GetAsync<Item>($"api/items/{itemId}")).GetAwaiter().GetResult());
    public OrderUserRepositoryFacade UsersRepository => new(
        () => Task.Run(async () => await GetAsync<List<User>>("api/users") ?? []).GetAwaiter().GetResult(),
        userId => Task.Run(async () => await GetAsync<User>($"api/users/{userId}")).GetAwaiter().GetResult());

    public async Task<IReadOnlyList<Order>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<Order>>(BaseUri) ?? [];

    public List<Order> GetAllOrders()
        => Task.Run(async () => await GetAsync<List<Order>>(BaseUri) ?? []).GetAwaiter().GetResult();

    public async Task<IReadOnlyList<Order>> GetOrdersByClientAsync(int clientId, CancellationToken cancellationToken = default)
        => await GetAsync<List<Order>>($"{BaseUri}?clientId={clientId}") ?? [];

    public List<Order> GetOrdersOfClient(int clientId)
        => Task.Run(async () => await GetAsync<List<Order>>($"{BaseUri}?clientId={clientId}") ?? []).GetAwaiter().GetResult();

    public async Task<Order?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default)
        => await GetAsync<Order>($"{BaseUri}/{orderId}");

    public Order? GetOrder(int orderId)
        => Task.Run(async () => await GetAsync<Order>($"{BaseUri}/{orderId}")).GetAwaiter().GetResult();

    public async Task<bool> OrderExistsAsync(int orderId, CancellationToken cancellationToken = default)
        => await GetAsync<bool>($"{BaseUri}/{orderId}/exists");

    public async Task<int> CreateOrderAsync(int clientId, DateOnly pickUpDate, bool isCompleted, bool isExpired, CancellationToken cancellationToken = default)
        => await PostAsync<object, int>(BaseUri, new { clientId, pickUpDate, isCompleted, isExpired });

    public async Task UpdateOrderAsync(Order order, CancellationToken cancellationToken = default)
        => await PutAsync($"{BaseUri}/{order.Id}", order);

    public void ModifyIncompleteOrder(int orderId, Dictionary<int, Tuple<int, float>> updatedItems, DateOnly pickUpDate)
        => Task.Run(async () => await PutAsync<object>($"{BaseUri}/{orderId}", new { updatedItems, pickUpDate })).GetAwaiter().GetResult();

    public async Task DeleteOrderAsync(int orderId, CancellationToken cancellationToken = default)
        => await DeleteAsync($"{BaseUri}/{orderId}");

    public async Task PlaceOrderFromBasketAsync(int userId, DateOnly chosenPickUpDate, CancellationToken cancellationToken = default)
        => await PostAsync<object, object>($"{BaseUri}/place-from-basket", new { userId, chosenPickUpDate });

    public async Task CompleteOrderAsync(int orderId, Dictionary<int, (int Quantity, float Discount)> updatedQuantities, CancellationToken cancellationToken = default)
        => await PostAsync<object, object>($"{BaseUri}/{orderId}/complete", new { updatedQuantities });

    public void CompleteOrder(int orderId, Dictionary<int, Tuple<int, float>> updatedItems)
        => Task.Run(async () => await PostAsync<object, object>($"{BaseUri}/{orderId}/complete", new { updatedItems })).GetAwaiter().GetResult();

    public async Task CancelOrderAsync(int orderId, CancellationToken cancellationToken = default)
        => await PostAsync<object, object>($"{BaseUri}/{orderId}/cancel", new { });

    public void CancelOrder(int orderId)
        => Task.Run(async () => await PostAsync<object, object>($"{BaseUri}/{orderId}/cancel", new { })).GetAwaiter().GetResult();

    public async Task ExpireOverdueOrdersAsync(CancellationToken cancellationToken = default)
        => await PostAsync<object, object>($"{BaseUri}/expire-overdue", new { });

    public void ExpireOverdueOrders()
        => Task.Run(async () => await PostAsync<object, object>($"{BaseUri}/expire-overdue", new { })).GetAwaiter().GetResult();

    public void ResubmitExpiredOrder(int orderId, DateOnly pickUpDate)
        => Task.Run(async () => await PostAsync<object, object>($"{BaseUri}/{orderId}/resubmit", new { pickUpDate })).GetAwaiter().GetResult();

    public async Task<List<BasketItemViewModel>> GetBasketItemsAsync(int userId, CancellationToken cancellationToken = default)
        => await GetAsync<List<BasketItemViewModel>>($"{BasketUri}?userId={userId}") ?? [];

    public Tuple<float, float> CalculateBasketTotalSum(List<BasketItemViewModel> basketItems)
        => new(0f, 0f);

    public async Task AddItemToBasketAsync(int userId, int itemId, int quantity, float extraDiscountPercentage = 0f, CancellationToken cancellationToken = default)
        => await PostAsync<object, object>($"{BasketUri}/add", new { userId, itemId, quantity, extraDiscountPercentage });

    public async Task UpdateBasketItemQuantityAsync(int userId, int itemId, int quantity, CancellationToken cancellationToken = default)
        => await PutAsync<object>($"{BasketUri}/update", new { userId, itemId, quantity });

    public async Task RemoveFromBasketAsync(int userId, int itemId, CancellationToken cancellationToken = default)
        => await DeleteAsync($"{BasketUri}/{itemId}?userId={userId}");

    public async Task ApplyPrescriptionToBasketAsync(int userId, string prescriptionId, CancellationToken cancellationToken = default)
        => await PostAsync<object, object>($"{BasketUri}/apply-prescription", new { userId, prescriptionId });
}
