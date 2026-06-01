using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class OrderService(
    IOrdersRepository ordersRepository,
    IItemsRepository itemsRepository,
    IUsersRepository usersRepository,
    IBasketRepository basketRepository) : IOrderService
{
    private const float MaximumDiscount = 1f;
    private const float MinimumDiscount = 0f;

    public async Task<IReadOnlyList<Order>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
        => await HydrateOrdersAsync(await ordersRepository.GetAllAsync());

    public async Task<IReadOnlyList<Order>> GetOrdersByClientAsync(int clientId, CancellationToken cancellationToken = default)
        => await HydrateOrdersAsync(await ordersRepository.GetByUserIdAsync(clientId));

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

    public async Task DeleteOrderAsync(int orderId, CancellationToken cancellationToken = default)
        => await ordersRepository.DeleteAsync(orderId);

    public async Task PlaceOrderFromBasketAsync(int userId, DateOnly chosenPickUpDate, CancellationToken cancellationToken = default)
    {
        var user = await usersRepository.GetUserByIdAsync(userId)
            ?? throw new ArgumentException("User not found.");
        var userDiscounts = (await usersRepository.GetUserDiscountsAsync(userId))
            .ToDictionary(discount => discount.Item.Id, discount => NormalizeDiscount(discount.DiscountPercentage));
        var basketEntries = await basketRepository.GetBasketByUserIdAsync(userId);

        var order = await ordersRepository.CreateAsync(new Order(0, user, chosenPickUpDate));
        foreach (var basketEntry in basketEntries)
        {
            var item = basketEntry.Item ?? await itemsRepository.GetByIdAsync(basketEntry.Item.Id);
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

    public async Task CancelOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await ordersRepository.GetByIdAsync(orderId)
            ?? throw new ArgumentException("Order not found.");
        order.IsExpired = true;
        await ordersRepository.UpdateAsync(order);
    }

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
}
