using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class OrderServiceTests
{
    private const int UserId = 3;
    private const int ItemId = 6;
    private const int OrderId = 12;
    private const int NonPositiveQuantity = 0;
    private const string NonNumericPrescriptionId = "abc";
    private static readonly DateOnly PickUpDate = new(2026, 7, 1);

    private static (OrderService Service, IOrdersRepository Orders, IItemsRepository Items, IUsersRepository Users, IBasketRepository Basket, IPrescriptionRepository Prescriptions) CreateService()
    {
        var orders = Substitute.For<IOrdersRepository>();
        var items = Substitute.For<IItemsRepository>();
        var users = Substitute.For<IUsersRepository>();
        var basket = Substitute.For<IBasketRepository>();
        var prescriptions = Substitute.For<IPrescriptionRepository>();
        return (new OrderService(orders, items, users, basket, prescriptions), orders, items, users, basket, prescriptions);
    }

    [TestMethod]
    public async Task CreateOrderAsync_ClientNotFound_ThrowsArgumentException()
    {
        var (service, _, _, users, _, _) = CreateService();
        users.GetUserByIdAsync(UserId).Returns((User?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CreateOrderAsync(UserId, PickUpDate, false, false));
    }

    [TestMethod]
    public async Task CancelOrderAsync_OrderNotFound_ThrowsArgumentException()
    {
        var (service, orders, _, _, _, _) = CreateService();
        orders.GetByIdAsync(OrderId).Returns((Order?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CancelOrderAsync(OrderId));
    }

    [TestMethod]
    public async Task CompleteOrderAsync_OrderNotFound_ThrowsArgumentException()
    {
        var (service, orders, _, _, _, _) = CreateService();
        orders.GetByIdAsync(OrderId).Returns((Order?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CompleteOrderAsync(OrderId, new Dictionary<int, (int Quantity, float Discount)>()));
    }

    [TestMethod]
    public async Task AddItemToBasketAsync_NonPositiveQuantity_ThrowsArgumentException()
    {
        var (service, _, _, _, _, _) = CreateService();

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.AddItemToBasketAsync(UserId, ItemId, NonPositiveQuantity));
    }

    [TestMethod]
    public async Task ApplyPrescriptionToBasketAsync_NonNumericId_ThrowsArgumentException()
    {
        var (service, _, _, _, _, _) = CreateService();

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.ApplyPrescriptionToBasketAsync(UserId, NonNumericPrescriptionId));
    }

    [TestMethod]
    public async Task UpdateBasketItemQuantityAsync_NonPositiveQuantity_RemovesItem()
    {
        var (service, _, _, _, basket, _) = CreateService();

        await service.UpdateBasketItemQuantityAsync(UserId, ItemId, NonPositiveQuantity);

        await basket.Received().RemoveFromBasketAsync(UserId, ItemId);
    }

    [TestMethod]
    public async Task UpdateOrderAsync_OrderNotFound_ThrowsArgumentException()
    {
        var (service, orders, _, _, _, _) = CreateService();
        orders.GetByIdAsync(OrderId).Returns((Order?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdateOrderAsync(new Order { Id = OrderId }));
    }

    private const int StockQuantity = 100;
    private const int RequestQuantity = 5;
    private const string PrescriptionId = "42";
    private static readonly DateOnly FutureExpiry = DateOnly.FromDateTime(DateTime.Today.AddYears(1));

    private static Item ItemWithStock() => new()
    {
        Id = ItemId,
        Name = "Aspirin",
        Price = 10f,
        ItemBatchEntries = [new ItemBatch { ExpirationDate = FutureExpiry, NumberOfPacks = StockQuantity }],
    };

    [TestMethod]
    public async Task GetAllOrdersAsync_ReturnsHydratedOrders()
    {
        var (service, orders, _, _, _, _) = CreateService();
        orders.GetAllAsync().Returns(new List<Order> { new() { Id = OrderId } });
        orders.GetOrderItemsByOrderIdAsync(OrderId).Returns(new List<OrderItem>());

        var result = await service.GetAllOrdersAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetOrderByIdAsync_NotFound_ReturnsNull()
    {
        var (service, orders, _, _, _, _) = CreateService();
        orders.GetByIdAsync(OrderId).Returns((Order?)null);

        var result = await service.GetOrderByIdAsync(OrderId);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task OrderExistsAsync_KnownOrder_ReturnsTrue()
    {
        var (service, orders, _, _, _, _) = CreateService();
        orders.GetByIdAsync(OrderId).Returns(new Order { Id = OrderId });

        bool exists = await service.OrderExistsAsync(OrderId);

        Assert.IsTrue(exists);
    }

    [TestMethod]
    public async Task CreateOrderAsync_Valid_ReturnsCreatedId()
    {
        var (service, orders, _, users, _, _) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId });
        orders.CreateAsync(Arg.Any<Order>()).Returns(new Order { Id = OrderId });

        int result = await service.CreateOrderAsync(UserId, PickUpDate, false, false);

        Assert.AreEqual(OrderId, result);
    }

    [TestMethod]
    public async Task DeleteOrderAsync_DelegatesToRepository()
    {
        var (service, orders, _, _, _, _) = CreateService();

        await service.DeleteOrderAsync(OrderId);

        await orders.Received().DeleteAsync(OrderId);
    }

    [TestMethod]
    public async Task AddItemToBasketAsync_UserNotFound_ThrowsArgumentException()
    {
        var (service, _, _, users, _, _) = CreateService();
        users.GetUserByIdAsync(UserId).Returns((User?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.AddItemToBasketAsync(UserId, ItemId, RequestQuantity));
    }

    [TestMethod]
    public async Task AddItemToBasketAsync_ItemNotFound_ThrowsArgumentException()
    {
        var (service, _, items, users, _, _) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId });
        items.GetByIdAsync(ItemId).Returns((Item?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.AddItemToBasketAsync(UserId, ItemId, RequestQuantity));
    }

    [TestMethod]
    public async Task AddItemToBasketAsync_InsufficientStock_ThrowsArgumentException()
    {
        var (service, _, items, users, basket, _) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId });
        items.GetByIdAsync(ItemId).Returns(new Item { Id = ItemId });
        basket.GetBasketEntryAsync(UserId, ItemId).Returns((BasketEntry?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.AddItemToBasketAsync(UserId, ItemId, RequestQuantity));
    }

    [TestMethod]
    public async Task AddItemToBasketAsync_NewEntry_AddsToBasket()
    {
        var (service, _, items, users, basket, _) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId });
        items.GetByIdAsync(ItemId).Returns(ItemWithStock());
        basket.GetBasketEntryAsync(UserId, ItemId).Returns((BasketEntry?)null);

        await service.AddItemToBasketAsync(UserId, ItemId, RequestQuantity);

        await basket.Received().AddToBasketAsync(Arg.Any<BasketEntry>());
    }

    [TestMethod]
    public async Task UpdateBasketItemQuantityAsync_EntryNotFound_ThrowsArgumentException()
    {
        var (service, _, _, _, basket, _) = CreateService();
        basket.GetBasketEntryAsync(UserId, ItemId).Returns((BasketEntry?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdateBasketItemQuantityAsync(UserId, ItemId, RequestQuantity));
    }

    [TestMethod]
    public async Task UpdateBasketItemQuantityAsync_Valid_UpdatesEntry()
    {
        var (service, _, _, _, basket, _) = CreateService();
        basket.GetBasketEntryAsync(UserId, ItemId).Returns(new BasketEntry(1) { Item = ItemWithStock() });

        await service.UpdateBasketItemQuantityAsync(UserId, ItemId, RequestQuantity);

        await basket.Received().UpdateBasketEntryAsync(Arg.Is<BasketEntry>(entry => entry.Quantity == RequestQuantity));
    }

    [TestMethod]
    public async Task RemoveFromBasketAsync_DelegatesToRepository()
    {
        var (service, _, _, _, basket, _) = CreateService();

        await service.RemoveFromBasketAsync(UserId, ItemId);

        await basket.Received().RemoveFromBasketAsync(UserId, ItemId);
    }

    [TestMethod]
    public async Task ApplyPrescriptionToBasketAsync_PrescriptionNotFound_ThrowsArgumentException()
    {
        var (service, _, _, _, _, prescriptions) = CreateService();
        prescriptions.GetByIdAsync(int.Parse(PrescriptionId)).Returns((Prescription?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.ApplyPrescriptionToBasketAsync(UserId, PrescriptionId));
    }

    [TestMethod]
    public async Task ApplyPrescriptionToBasketAsync_NoMedications_ThrowsArgumentException()
    {
        var (service, _, _, _, _, prescriptions) = CreateService();
        prescriptions.GetByIdAsync(int.Parse(PrescriptionId)).Returns(new Prescription { MedicationList = [] });

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.ApplyPrescriptionToBasketAsync(UserId, PrescriptionId));
    }

    [TestMethod]
    public async Task ExpireOverdueOrdersAsync_OverdueOrder_MarksExpired()
    {
        var (service, orders, _, _, _, _) = CreateService();
        var overdue = new Order { Id = OrderId, PickUpDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-30)) };
        orders.GetAllAsync().Returns(new List<Order> { overdue });

        await service.ExpireOverdueOrdersAsync();

        await orders.Received().UpdateAsync(Arg.Is<Order>(order => order.IsExpired));
    }

    [TestMethod]
    public void CalculateBasketTotalSum_SumsBeforeDiscount()
    {
        var (service, _, _, _, _, _) = CreateService();
        var item = new BasketItemViewModel(ItemId, string.Empty, "Aspirin", "Acme", 1, 0f, 0f, 0f, 10f);
        item.SetFinalPrices(10f, 8f);

        var total = service.CalculateBasketTotalSum([item]);

        Assert.AreEqual(10f, total.Item1);
    }

    [TestMethod]
    public async Task PlaceOrderFromBasketAsync_Valid_ClearsBasket()
    {
        var (service, orders, _, users, basket, _) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId });
        users.GetUserDiscountsAsync(UserId).Returns(new List<UserDiscount>());
        basket.GetBasketByUserIdAsync(UserId).Returns(new List<BasketEntry> { new(RequestQuantity) { Item = ItemWithStock() } });
        orders.CreateAsync(Arg.Any<Order>()).Returns(new Order { Id = OrderId });

        await service.PlaceOrderFromBasketAsync(UserId, PickUpDate);

        await basket.Received().ClearBasketAsync(UserId);
    }

    [TestMethod]
    public async Task GetBasketItemsAsync_Valid_ReturnsViewModels()
    {
        var (service, _, _, users, basket, _) = CreateService();
        users.GetUserDiscountsAsync(UserId).Returns(new List<UserDiscount>());
        basket.GetBasketByUserIdAsync(UserId).Returns(new List<BasketEntry> { new(RequestQuantity) { Item = ItemWithStock() } });

        var result = await service.GetBasketItemsAsync(UserId);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task UpdateOrderAsync_Valid_AddsOrderItems()
    {
        var (service, orders, items, _, _, _) = CreateService();
        var order = new Order { Id = OrderId };
        order.ItemQuantitiesWithFinalPrice = new Dictionary<int, Tuple<int, float>> { { ItemId, Tuple.Create(RequestQuantity, 10f) } };
        orders.GetByIdAsync(OrderId).Returns(new Order { Id = OrderId });
        orders.GetOrderItemsByOrderIdAsync(OrderId).Returns(new List<OrderItem>());
        items.GetByIdAsync(ItemId).Returns(ItemWithStock());

        await service.UpdateOrderAsync(order);

        await orders.Received().AddOrderItemAsync(Arg.Any<OrderItem>());
    }

    [TestMethod]
    public async Task CompleteOrderAsync_Valid_UpdatesItemStock()
    {
        var (service, orders, items, _, _, _) = CreateService();
        orders.GetByIdAsync(OrderId).Returns(new Order { Id = OrderId });
        orders.GetOrderItemsByOrderIdAsync(OrderId).Returns(new List<OrderItem>());
        items.GetByIdAsync(ItemId).Returns(ItemWithStock());
        var updated = new Dictionary<int, (int Quantity, float Discount)> { { ItemId, (RequestQuantity, 0f) } };

        await service.CompleteOrderAsync(OrderId, updated);

        await items.Received().UpdateAsync(Arg.Any<Item>());
    }

    [TestMethod]
    public async Task GetOrdersByClientAsync_ReturnsHydratedOrders()
    {
        var (service, orders, _, _, _, _) = CreateService();
        orders.GetByUserIdAsync(UserId).Returns(new List<Order> { new() { Id = OrderId } });
        orders.GetOrderItemsByOrderIdAsync(OrderId).Returns(new List<OrderItem>());

        var result = await service.GetOrdersByClientAsync(UserId);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void GetAllOrders_ReturnsList()
    {
        var (service, orders, _, _, _, _) = CreateService();
        orders.GetAllAsync().Returns(new List<Order> { new() { Id = OrderId } });
        orders.GetOrderItemsByOrderIdAsync(OrderId).Returns(new List<OrderItem>());

        var result = service.GetAllOrders();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void GetOrder_ReturnsOrder()
    {
        var (service, orders, _, _, _, _) = CreateService();
        orders.GetByIdAsync(OrderId).Returns(new Order { Id = OrderId });
        orders.GetOrderItemsByOrderIdAsync(OrderId).Returns(new List<OrderItem>());

        var result = service.GetOrder(OrderId);

        Assert.AreEqual(OrderId, result!.Id);
    }

    [TestMethod]
    public void CancelOrder_DelegatesToRepository()
    {
        var (service, orders, _, _, _, _) = CreateService();
        orders.GetByIdAsync(OrderId).Returns(new Order { Id = OrderId });

        service.CancelOrder(OrderId);

        orders.Received().UpdateAsync(Arg.Is<Order>(order => order.IsExpired));
    }

    [TestMethod]
    public void ExpireOverdueOrders_DelegatesToRepository()
    {
        var (service, orders, _, _, _, _) = CreateService();
        orders.GetAllAsync().Returns(new List<Order>());

        service.ExpireOverdueOrders();

        orders.Received().GetAllAsync();
    }

    [TestMethod]
    public void OrdersRepositoryFacade_IsAvailable()
    {
        var (service, _, _, _, _, _) = CreateService();

        var facade = service.OrdersRepository;

        Assert.IsNotNull(facade);
    }

    [TestMethod]
    public void ItemsRepositoryFacade_IsAvailable()
    {
        var (service, _, _, _, _, _) = CreateService();

        Assert.IsNotNull(service.ItemsRepository);
    }

    [TestMethod]
    public void UsersRepositoryFacade_IsAvailable()
    {
        var (service, _, _, _, _, _) = CreateService();

        Assert.IsNotNull(service.UsersRepository);
    }

    [TestMethod]
    public void GetOrdersOfClient_ReturnsList()
    {
        var (service, orders, _, _, _, _) = CreateService();
        orders.GetByUserIdAsync(UserId).Returns(new List<Order> { new() { Id = OrderId } });
        orders.GetOrderItemsByOrderIdAsync(OrderId).Returns(new List<OrderItem>());

        var result = service.GetOrdersOfClient(UserId);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task AddItemToBasketAsync_ExistingEntry_UpdatesEntry()
    {
        var (service, _, items, users, basket, _) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId });
        items.GetByIdAsync(ItemId).Returns(ItemWithStock());
        basket.GetBasketEntryAsync(UserId, ItemId).Returns(new BasketEntry(1) { Item = ItemWithStock() });

        await service.AddItemToBasketAsync(UserId, ItemId, RequestQuantity);

        await basket.Received().UpdateBasketEntryAsync(Arg.Any<BasketEntry>());
    }

    [TestMethod]
    public async Task UpdateBasketItemQuantityAsync_InsufficientStock_ThrowsArgumentException()
    {
        var (service, _, _, _, basket, _) = CreateService();
        basket.GetBasketEntryAsync(UserId, ItemId).Returns(new BasketEntry(1) { Item = new Item { Id = ItemId } });

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdateBasketItemQuantityAsync(UserId, ItemId, RequestQuantity));
    }

    [TestMethod]
    public async Task ApplyPrescriptionToBasketAsync_MatchingCatalogueItem_AddsToBasket()
    {
        var (service, _, items, users, basket, prescriptions) = CreateService();
        prescriptions.GetByIdAsync(int.Parse(PrescriptionId)).Returns(new Prescription
        {
            MedicationList = [new PrescriptionItem { MedicationName = "Aspirin", Quantity = "2" }],
        });
        items.GetAllAsync().Returns(new List<Item> { ItemWithStock() });
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId });
        items.GetByIdAsync(ItemId).Returns(ItemWithStock());
        basket.GetBasketEntryAsync(UserId, ItemId).Returns((BasketEntry?)null);

        await service.ApplyPrescriptionToBasketAsync(UserId, PrescriptionId);

        await basket.Received().AddToBasketAsync(Arg.Any<BasketEntry>());
    }
}
