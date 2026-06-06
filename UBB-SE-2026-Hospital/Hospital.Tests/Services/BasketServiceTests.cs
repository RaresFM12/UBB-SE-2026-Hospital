using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class BasketServiceTests
{
    private const int UserId = 4;
    private const int ItemId = 9;
    private const int ExistingQuantity = 2;
    private const int AddedQuantity = 3;
    private const int ExpectedMergedQuantity = ExistingQuantity + AddedQuantity;

    private static (BasketService Service, IBasketRepository Basket, IUsersRepository Users, IItemsRepository Items) CreateService()
    {
        var basket = Substitute.For<IBasketRepository>();
        var users = Substitute.For<IUsersRepository>();
        var items = Substitute.For<IItemsRepository>();
        return (new BasketService(basket, users, items), basket, users, items);
    }

    [TestMethod]
    public async Task AddToBasketAsync_ExistingEntry_MergesQuantity()
    {
        var (service, basket, _, _) = CreateService();
        basket.GetBasketEntryAsync(UserId, ItemId).Returns(new BasketEntry(ExistingQuantity) { Item = new Item { Id = ItemId } });

        await service.AddToBasketAsync(UserId, ItemId, AddedQuantity);

        await basket.Received().UpdateBasketEntryAsync(Arg.Is<BasketEntry>(entry => entry.Quantity == ExpectedMergedQuantity));
    }

    [TestMethod]
    public async Task AddToBasketAsync_NewEntryWithUnknownUser_ThrowsArgumentException()
    {
        var (service, basket, users, _) = CreateService();
        basket.GetBasketEntryAsync(UserId, ItemId).Returns((BasketEntry?)null);
        users.GetUserByIdAsync(UserId).Returns((User?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.AddToBasketAsync(UserId, ItemId, AddedQuantity));
    }

    [TestMethod]
    public async Task AddToBasketAsync_NewEntryWithUnknownItem_ThrowsArgumentException()
    {
        var (service, basket, users, items) = CreateService();
        basket.GetBasketEntryAsync(UserId, ItemId).Returns((BasketEntry?)null);
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId });
        items.GetByIdAsync(ItemId).Returns((Item?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.AddToBasketAsync(UserId, ItemId, AddedQuantity));
    }

    [TestMethod]
    public async Task ClearBasketAsync_DelegatesToRepository()
    {
        var (service, basket, _, _) = CreateService();

        await service.ClearBasketAsync(UserId);

        await basket.Received().ClearBasketAsync(UserId);
    }

    [TestMethod]
    public async Task GetBasketAsync_MapsRepositoryEntries()
    {
        var (service, basket, _, _) = CreateService();
        basket.GetBasketByUserIdAsync(UserId).Returns(new List<BasketEntry>
        {
            new(ExistingQuantity) { Item = new Item { Id = ItemId } },
        });

        var result = await service.GetBasketAsync(UserId);

        Assert.AreEqual(ExistingQuantity, result[ItemId].Quantity);
    }

    [TestMethod]
    public async Task SaveBasketAsync_ClearsExistingBasket()
    {
        var (service, basket, users, items) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId });
        items.GetByIdAsync(ItemId).Returns(new Item { Id = ItemId });
        basket.GetBasketEntryAsync(UserId, ItemId).Returns((BasketEntry?)null);
        var entries = new Dictionary<int, BasketEntryDto>
        {
            { ItemId, new BasketEntryDto { ItemId = ItemId, Quantity = AddedQuantity } },
        };

        await service.SaveBasketAsync(UserId, entries);

        await basket.Received().ClearBasketAsync(UserId);
    }

    [TestMethod]
    public async Task AddToBasketAsync_NewEntryWithValidData_AddsToBasket()
    {
        var (service, basket, users, items) = CreateService();
        users.GetUserByIdAsync(UserId).Returns(new User { Id = UserId });
        items.GetByIdAsync(ItemId).Returns(new Item { Id = ItemId });
        basket.GetBasketEntryAsync(UserId, ItemId).Returns((BasketEntry?)null);

        await service.AddToBasketAsync(UserId, ItemId, AddedQuantity);

        await basket.Received().AddToBasketAsync(Arg.Any<BasketEntry>());
    }
}
