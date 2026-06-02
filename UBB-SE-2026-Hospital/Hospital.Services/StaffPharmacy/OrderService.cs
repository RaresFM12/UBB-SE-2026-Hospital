using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Models.Orders;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class OrderService(
    IOrdersRepository ordersRepository,
    IItemsRepository itemsRepository,
    IUsersRepository usersRepository,
    IBasketRepository basketRepository,
    IPrescriptionRepository prescriptionRepository) : IOrderService
{
    private const float MaximumDiscount = 1f;
    private const float MinimumDiscount = 0f;
    private const int DefaultPrescriptionQuantity = 1;

    public OrderRepositoryFacade OrdersRepository => new(this);

    public OrderItemRepositoryFacade ItemsRepository => new(itemId => itemsRepository.GetByIdAsync(itemId).GetAwaiter().GetResult());

    public OrderUserRepositoryFacade UsersRepository => new(
        () => usersRepository.GetAllUsersAsync().GetAwaiter().GetResult(),
        userId => usersRepository.GetUserByIdAsync(userId).GetAwaiter().GetResult());

    public async Task<IReadOnlyList<Order>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
        => await HydrateOrdersAsync(await ordersRepository.GetAllAsync());

    public List<Order> GetAllOrders() =>
        GetAllOrdersAsync().GetAwaiter().GetResult().ToList();

    public async Task<IReadOnlyList<Order>> GetOrdersByClientAsync(int clientId, CancellationToken cancellationToken = default)
        => await HydrateOrdersAsync(await ordersRepository.GetByUserIdAsync(clientId));

    public List<Order> GetOrdersOfClient(int clientId) =>
        GetOrdersByClientAsync(clientId).GetAwaiter().GetResult().ToList();

    public async Task<Order?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await ordersRepository.GetByIdAsync(orderId);
        if (order is null)
        {
            return null;
        }

        await HydrateOrderAsync(order);
        return order;
    }

    public Order? GetOrder(int orderId) =>
        GetOrderByIdAsync(orderId).GetAwaiter().GetResult();

    public async Task<bool> OrderExistsAsync(int orderId, CancellationToken cancellationToken = default)
        => await ordersRepository.GetByIdAsync(orderId) is not null;

    public async Task<int> CreateOrderAsync(int clientId, DateOnly pickUpDate, bool isCompleted, bool isExpired, CancellationToken cancellationToken = default)
    {
        var user = await usersRepository.GetUserByIdAsync(clientId)
            ?? throw new ArgumentException("Client not found.");

        var order = await ordersRepository.CreateAsync(new Order(0, user, pickUpDate, isCompleted, isExpired));
        return order.Id;
    }

    public async Task UpdateOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        var existing = await ordersRepository.GetByIdAsync(order.Id)
            ?? throw new ArgumentException("Order not found.");

        existing.PickUpDate = order.PickUpDate;
        existing.IsCompleted = order.IsCompleted;
        existing.IsExpired = order.IsExpired;
        await ordersRepository.UpdateAsync(existing);

        var currentOrderItems = await ordersRepository.GetOrderItemsByOrderIdAsync(order.Id);
        foreach (var orderItem in currentOrderItems)
        {
            await ordersRepository.DeleteOrderItemAsync(orderItem.Id);
        }

        foreach (var itemEntry in order.ItemQuantitiesWithFinalPrice)
        {
            var item = await itemsRepository.GetByIdAsync(itemEntry.Key)
                ?? throw new ArgumentException($"Item #{itemEntry.Key} not found.");

            await ordersRepository.AddOrderItemAsync(new OrderItem
            {
                Order = existing,
                Item = item,
                OrderQuantity = itemEntry.Value.Item1,
                Price = itemEntry.Value.Item2,
            });
        }
    }

    public void ModifyIncompleteOrder(int orderId, Dictionary<int, Tuple<int, float>> updatedItems, DateOnly pickUpDate)
    {
        var order = GetOrderByIdAsync(orderId).GetAwaiter().GetResult()
            ?? throw new ArgumentException("Order not found.");
        order.PickUpDate = pickUpDate;
        order.ItemQuantitiesWithFinalPrice = updatedItems;
        UpdateOrderAsync(order).GetAwaiter().GetResult();
    }

    public async Task DeleteOrderAsync(int orderId, CancellationToken cancellationToken = default)
        => await ordersRepository.DeleteAsync(orderId);

    public async Task PlaceOrderFromBasketAsync(int userId, DateOnly chosenPickUpDate, CancellationToken cancellationToken = default)
    {
        var user = await usersRepository.GetUserByIdAsync(userId)
            ?? throw new ArgumentException("User not found.");
        var userDiscounts = (await usersRepository.GetUserDiscountsAsync(userId))
            .Where(discount => discount.Item is not null)
            .ToDictionary(discount => discount.Item!.Id, discount => NormalizeDiscount(discount.DiscountPercentage));
        var basketEntries = await basketRepository.GetBasketByUserIdAsync(userId);

        var order = await ordersRepository.CreateAsync(new Order(0, user, chosenPickUpDate));
        foreach (var basketEntry in basketEntries)
        {
            var item = basketEntry.Item;
            if (item is null)
            {
                continue;
            }

            HydrateItem(item);
            int availableQuantity = item.GetQuantityAtSpecifiedDate(chosenPickUpDate);
            if (basketEntry.Quantity > availableQuantity)
            {
                throw new ArgumentException(
                    $"On {chosenPickUpDate:yyyy.MM.dd}, we will have only {availableQuantity} boxes of {item.Name} by {item.Producer} instead of {basketEntry.Quantity}.");
            }

            float finalPrice = CalculateFinalPrice(item, basketEntry.Quantity, basketEntry.ExtraDiscountPercentage, userDiscounts.GetValueOrDefault(item.Id));
            await ordersRepository.AddOrderItemAsync(new OrderItem
            {
                Order = order,
                Item = item,
                OrderQuantity = basketEntry.Quantity,
                Price = finalPrice,
            });
        }

        await basketRepository.ClearBasketAsync(userId);
    }

    public async Task CompleteOrderAsync(int orderId, Dictionary<int, (int Quantity, float Discount)> updatedQuantities, CancellationToken cancellationToken = default)
    {
        var order = await ordersRepository.GetByIdAsync(orderId)
            ?? throw new ArgumentException("Order not found.");

        foreach (var entry in updatedQuantities)
        {
            var item = await itemsRepository.GetByIdAsync(entry.Key)
                ?? throw new ArgumentException($"Item #{entry.Key} not found.");
            HydrateItem(item);

            if (item.GetQuantityAtSpecifiedDate(DateOnly.FromDateTime(DateTime.Now)) < entry.Value.Quantity)
            {
                throw new ArgumentException($"We don't have enough of {item.Name} - {item.Producer}.");
            }
        }

        order.IsCompleted = true;
        order.ItemQuantitiesWithFinalPrice = updatedQuantities.ToDictionary(
            pair => pair.Key,
            pair => Tuple.Create(pair.Value.Quantity, pair.Value.Discount));
        await UpdateOrderAsync(order, cancellationToken);

        foreach (var entry in updatedQuantities)
        {
            var item = await itemsRepository.GetByIdAsync(entry.Key);
            if (item is null)
            {
                continue;
            }

            HydrateItem(item);
            item.RemoveQuantityFromItem(entry.Value.Quantity, DateOnly.FromDateTime(DateTime.Now));
            item.Quantity = item.Batches.Values.Sum();
            item.ItemBatchEntries = item.Batches.Select(batch => new ItemBatch
            {
                Item = item,
                ExpirationDate = batch.Key,
                NumberOfPacks = batch.Value,
            }).ToList();
            await itemsRepository.UpdateAsync(item);
        }
    }

    public void CompleteOrder(int orderId, Dictionary<int, Tuple<int, float>> updatedItems) =>
        CompleteOrderAsync(
            orderId,
            updatedItems.ToDictionary(pair => pair.Key, pair => (pair.Value.Item1, pair.Value.Item2)))
            .GetAwaiter().GetResult();

    public async Task CancelOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await ordersRepository.GetByIdAsync(orderId)
            ?? throw new ArgumentException("Order not found.");
        order.IsExpired = true;
        await ordersRepository.UpdateAsync(order);
    }

    public void CancelOrder(int orderId) =>
        CancelOrderAsync(orderId).GetAwaiter().GetResult();

    public async Task ExpireOverdueOrdersAsync(CancellationToken cancellationToken = default)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        foreach (var order in await ordersRepository.GetAllAsync())
        {
            if (!order.IsExpired && !order.IsCompleted && today > order.PickUpDate.AddDays(Order.OrderExpirationDays))
            {
                order.IsExpired = true;
                await ordersRepository.UpdateAsync(order);
            }
        }
    }

    public void ExpireOverdueOrders() =>
        ExpireOverdueOrdersAsync().GetAwaiter().GetResult();

    public void ResubmitExpiredOrder(int orderId, DateOnly pickUpDate)
    {
        var order = GetOrderByIdAsync(orderId).GetAwaiter().GetResult()
            ?? throw new ArgumentException("Order not found.");
        order.PickUpDate = pickUpDate;
        order.IsExpired = false;
        UpdateOrderAsync(order).GetAwaiter().GetResult();
    }

    public async Task<List<BasketItemViewModel>> GetBasketItemsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var userDiscounts = (await usersRepository.GetUserDiscountsAsync(userId))
            .Where(discount => discount.Item is not null)
            .ToDictionary(discount => discount.Item!.Id, discount => NormalizeDiscount(discount.DiscountPercentage));
        var entries = await basketRepository.GetBasketByUserIdAsync(userId);
        var basketItems = new List<BasketItemViewModel>();

        foreach (var entry in entries)
        {
            if (entry.Item is null)
            {
                continue;
            }

            HydrateItem(entry.Item);
            float userDiscount = userDiscounts.GetValueOrDefault(entry.Item.Id);
            var item = new BasketItemViewModel(
                entry.Item.Id,
                entry.Item.ImagePath,
                entry.Item.Name,
                entry.Item.Producer,
                entry.Quantity,
                NormalizeDiscount(entry.Item.DiscountPercentage),
                NormalizeDiscount(entry.ExtraDiscountPercentage),
                userDiscount,
                entry.Item.Price);
            item.SetFinalPrices(
                entry.Item.Price * entry.Quantity,
                CalculateFinalPrice(entry.Item, entry.Quantity, entry.ExtraDiscountPercentage, userDiscount));
            basketItems.Add(item);
        }

        return basketItems;
    }

    public Tuple<float, float> CalculateBasketTotalSum(List<BasketItemViewModel> basketItems) =>
        Tuple.Create(
            basketItems.Sum(item => item.FinalPriceBeforeDiscount),
            basketItems.Sum(item => item.FinalPriceAfterDiscount));

    public async Task AddItemToBasketAsync(int userId, int itemId, int quantity, float extraDiscountPercentage = 0f, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be positive.");
        }

        var user = await usersRepository.GetUserByIdAsync(userId)
            ?? throw new ArgumentException("User not found.");
        var item = await itemsRepository.GetByIdAsync(itemId)
            ?? throw new ArgumentException("Item not found.");
        HydrateItem(item);

        var existing = await basketRepository.GetBasketEntryAsync(userId, itemId);
        int requestedQuantity = quantity + (existing?.Quantity ?? 0);
        if (requestedQuantity > item.GetQuantityAtSpecifiedDate(DateOnly.FromDateTime(DateTime.Today)))
        {
            throw new ArgumentException("Requested quantity is not available in stock.");
        }

        if (existing is not null)
        {
            existing.Quantity = requestedQuantity;
            existing.ExtraDiscountPercentage = Math.Max(existing.ExtraDiscountPercentage, extraDiscountPercentage);
            await basketRepository.UpdateBasketEntryAsync(existing);
            return;
        }

        await basketRepository.AddToBasketAsync(new BasketEntry(quantity, extraDiscountPercentage)
        {
            User = user,
            Item = item,
        });
    }

    public async Task UpdateBasketItemQuantityAsync(int userId, int itemId, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            await basketRepository.RemoveFromBasketAsync(userId, itemId);
            return;
        }

        var entry = await basketRepository.GetBasketEntryAsync(userId, itemId)
            ?? throw new ArgumentException("Basket item not found.");
        var item = entry.Item ?? await itemsRepository.GetByIdAsync(itemId)
            ?? throw new ArgumentException("Item not found.");
        HydrateItem(item);

        if (quantity > item.GetQuantityAtSpecifiedDate(DateOnly.FromDateTime(DateTime.Today)))
        {
            throw new ArgumentException("Requested quantity is not available in stock.");
        }

        entry.Quantity = quantity;
        await basketRepository.UpdateBasketEntryAsync(entry);
    }

    public async Task RemoveFromBasketAsync(int userId, int itemId, CancellationToken cancellationToken = default)
    {
        await basketRepository.RemoveFromBasketAsync(userId, itemId);
    }

    public async Task ApplyPrescriptionToBasketAsync(int userId, string prescriptionId, CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(prescriptionId, out int id))
        {
            throw new ArgumentException("Prescription ID must be numeric.");
        }

        var prescription = await prescriptionRepository.GetByIdAsync(id)
            ?? throw new ArgumentException("Prescription not found.");
        if (prescription.MedicationList.Count == 0)
        {
            throw new ArgumentException("Prescription has no medication items.");
        }

        var catalogueItems = await itemsRepository.GetAllAsync();
        foreach (var medication in prescription.MedicationList)
        {
            var item = catalogueItems.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, medication.MedicationName, StringComparison.OrdinalIgnoreCase));
            if (item is null)
            {
                throw new ArgumentException($"No catalogue item matches {medication.MedicationName}.");
            }

            await AddItemToBasketAsync(userId, item.Id, ParsePrescriptionQuantity(medication.Quantity), cancellationToken: cancellationToken);
        }
    }

    private async Task<IReadOnlyList<Order>> HydrateOrdersAsync(List<Order> orders)
    {
        foreach (var order in orders)
        {
            await HydrateOrderAsync(order);
        }

        return orders;
    }

    private async Task HydrateOrderAsync(Order order)
    {
        var orderItems = await ordersRepository.GetOrderItemsByOrderIdAsync(order.Id);
        order.ItemQuantitiesWithFinalPrice = orderItems
            .Where(orderItem => orderItem.Item is not null)
            .ToDictionary(orderItem => orderItem.Item.Id, orderItem => Tuple.Create(orderItem.OrderQuantity, orderItem.Price));
    }

    private static void HydrateItem(Item item)
    {
        item.Batches = item.ItemBatchEntries.ToDictionary(entry => entry.ExpirationDate, entry => entry.NumberOfPacks);
    }

    private static float NormalizeDiscount(float discount)
    {
        if (discount > MaximumDiscount)
        {
            discount /= 100f;
        }

        return Math.Clamp(discount, MinimumDiscount, MaximumDiscount);
    }

    private static float CalculateFinalPrice(Item item, int quantity, float extraDiscount, float userDiscount)
    {
        float price = item.Price * quantity;
        price *= MaximumDiscount - NormalizeDiscount(item.DiscountPercentage);
        price *= MaximumDiscount - NormalizeDiscount(extraDiscount);
        price *= MaximumDiscount - NormalizeDiscount(userDiscount);
        return price;
    }

    private static int ParsePrescriptionQuantity(string? quantity)
    {
        if (string.IsNullOrWhiteSpace(quantity))
        {
            return DefaultPrescriptionQuantity;
        }

        string digits = new(quantity.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out int parsedQuantity) && parsedQuantity > 0
            ? parsedQuantity
            : DefaultPrescriptionQuantity;
    }
}
