using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services;
using NSubstitute;

namespace Hospital.Tests.Services;

[TestClass]
public class AdminServiceTests
{
    private const string SubstanceName = "Morphine";
    private const float LethalDose = 200f;
    private const string Description = "Opioid";
    private const int ItemId = 5;
    private const string MatchingItemName = "Aspirin";
    private const string OtherItemName = "Bandage";

    private static (AdminService Service, IItemsRepository Items, ISubstancesRepository Substances, IHighRiskMedicineRepository HighRisk, IOrdersRepository Orders) CreateService()
    {
        var items = Substitute.For<IItemsRepository>();
        var substances = Substitute.For<ISubstancesRepository>();
        var highRisk = Substitute.For<IHighRiskMedicineRepository>();
        var orders = Substitute.For<IOrdersRepository>();
        return (new AdminService(items, substances, highRisk, orders), items, substances, highRisk, orders);
    }

    [TestMethod]
    public async Task CreateSubstanceAsync_AlreadyExists_ThrowsArgumentException()
    {
        var (service, _, substances, _, _) = CreateService();
        substances.GetAllAsync().Returns(new List<Substance> { new() { Name = SubstanceName } });

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.CreateSubstanceAsync(SubstanceName, LethalDose, Description));
    }

    [TestMethod]
    public async Task UpdateSubstanceAsync_NotExisting_ThrowsArgumentException()
    {
        var (service, _, substances, _, _) = CreateService();
        substances.GetAllAsync().Returns(new List<Substance>());

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdateSubstanceAsync(new Substance { Name = SubstanceName }));
    }

    [TestMethod]
    public async Task DeleteSubstanceAsync_NotFound_ThrowsArgumentException()
    {
        var (service, _, substances, _, _) = CreateService();
        substances.GetAllAsync().Returns(new List<Substance>());

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.DeleteSubstanceAsync(SubstanceName));
    }

    [TestMethod]
    public async Task GetSubstanceByNameAsync_ReturnsMatch()
    {
        var (service, _, substances, _, _) = CreateService();
        substances.GetAllAsync().Returns(new List<Substance> { new() { Name = SubstanceName } });

        var result = await service.GetSubstanceByNameAsync(SubstanceName);

        Assert.AreEqual(SubstanceName, result!.Name);
    }

    [TestMethod]
    public async Task SubstanceExistsAsync_NoMatch_ReturnsFalse()
    {
        var (service, _, substances, _, _) = CreateService();
        substances.GetAllAsync().Returns(new List<Substance>());

        bool exists = await service.SubstanceExistsAsync(SubstanceName);

        Assert.IsFalse(exists);
    }

    [TestMethod]
    public async Task UpdateItemAsync_ItemDoesNotExist_ThrowsArgumentException()
    {
        var (service, items, _, _, _) = CreateService();
        items.GetByIdNoTrackingAsync(ItemId).Returns((Item?)null);

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => service.UpdateItemAsync(new Item { Id = ItemId, Name = MatchingItemName }));
    }

    [TestMethod]
    public async Task SearchItemsByName_FiltersByName()
    {
        var (service, items, _, _, _) = CreateService();
        items.GetAllAsync().Returns(new List<Item>
        {
            new() { Id = 1, Name = MatchingItemName },
            new() { Id = 2, Name = OtherItemName },
        });

        var result = await Task.FromResult(service.SearchItemsByName(MatchingItemName));

        Assert.HasCount(1, result);
    }

    private const string Producer = "Acme";
    private const string Category = "general";
    private const float Price = 10f;
    private const int Pills = 10;
    private const int OrderId = 3;
    private const int OrderQuantity = 2;

    [TestMethod]
    public async Task GetItemByIdAsync_ReturnsItem()
    {
        var (service, items, _, _, _) = CreateService();
        items.GetByIdAsync(ItemId).Returns(new Item { Id = ItemId, Name = MatchingItemName });

        var result = await service.GetItemByIdAsync(ItemId);

        Assert.AreEqual(ItemId, result!.Id);
    }

    [TestMethod]
    public async Task ItemExistsAsync_KnownItem_ReturnsTrue()
    {
        var (service, items, _, _, _) = CreateService();
        items.GetByIdAsync(ItemId).Returns(new Item { Id = ItemId });

        bool exists = await service.ItemExistsAsync(ItemId);

        Assert.IsTrue(exists);
    }

    [TestMethod]
    public async Task GetItemsAsync_FilteredOverload_ReturnsMatchingItems()
    {
        var (service, items, _, _, _) = CreateService();
        items.GetAllAsync().Returns(new List<Item> { new() { Id = ItemId, Name = MatchingItemName } });

        var result = await service.GetItemsAsync(search: MatchingItemName);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetTopItemsAsync_AggregatesOrderQuantities()
    {
        var (service, items, _, _, orders) = CreateService();
        var item = new Item { Id = ItemId, Name = MatchingItemName };
        items.GetAllAsync().Returns(new List<Item> { item });
        orders.GetAllAsync().Returns(new List<Order> { new() { Id = OrderId } });
        orders.GetOrderItemsByOrderIdAsync(OrderId).Returns(new List<OrderItem> { new() { Item = item, OrderQuantity = OrderQuantity } });

        var result = await service.GetTopItemsAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task CreateItemAsync_Valid_CreatesItem()
    {
        var (service, items, _, _, _) = CreateService();
        items.GetAllAsync().Returns(new List<Item>());

        await service.CreateItemAsync(MatchingItemName, Producer, Category, Price, Pills, string.Empty, string.Empty, string.Empty, 0f);

        await items.Received().CreateAsync(Arg.Is<Item>(item => item.Name == MatchingItemName));
    }

    [TestMethod]
    public async Task DeleteItemAsync_DelegatesToRepository()
    {
        var (service, items, _, _, _) = CreateService();

        await service.DeleteItemAsync(ItemId);

        await items.Received().DeleteAsync(ItemId);
    }

    [TestMethod]
    public async Task UpdateItemAsync_ExistingItem_UpdatesWithEntries()
    {
        var (service, items, _, _, _) = CreateService();
        items.GetByIdNoTrackingAsync(ItemId).Returns(new Item { Id = ItemId });

        await service.UpdateItemAsync(new Item { Id = ItemId, Name = MatchingItemName });

        await items.Received().UpdateWithEntriesAsync(Arg.Any<Item>());
    }

    [TestMethod]
    public async Task GetSubstancesAsync_ReturnsRepositoryResult()
    {
        var (service, _, substances, _, _) = CreateService();
        substances.GetAllAsync().Returns(new List<Substance> { new() { Name = SubstanceName } });

        var result = await service.GetSubstancesAsync();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetTopSubstancesAsync_CountsSubstanceUsage()
    {
        var (service, items, _, _, _) = CreateService();
        items.GetAllAsync().Returns(new List<Item>
        {
            new() { Id = ItemId, ItemSubstanceEntries = [new ItemSubstance { Substance = new Substance { Name = SubstanceName }, Concentration = 1f }] },
        });

        var result = await service.GetTopSubstancesAsync();

        Assert.AreEqual(1, result[SubstanceName]);
    }

    [TestMethod]
    public async Task CreateSubstanceAsync_New_CreatesSubstance()
    {
        var (service, _, substances, _, _) = CreateService();
        substances.GetAllAsync().Returns(new List<Substance>());

        await service.CreateSubstanceAsync(SubstanceName, LethalDose, Description);

        await substances.Received().CreateAsync(Arg.Is<Substance>(substance => substance.Name == SubstanceName));
    }

    [TestMethod]
    public async Task UpdateSubstanceAsync_Existing_UpdatesSubstance()
    {
        var (service, _, substances, _, _) = CreateService();
        substances.GetAllAsync().Returns(new List<Substance> { new() { Name = SubstanceName } });

        await service.UpdateSubstanceAsync(new Substance { Name = SubstanceName });

        await substances.Received().UpdateAsync(Arg.Any<Substance>());
    }

    [TestMethod]
    public async Task DeleteSubstanceAsync_Existing_DeletesSubstance()
    {
        var (service, _, substances, _, _) = CreateService();
        substances.GetAllAsync().Returns(new List<Substance> { new() { Id = ItemId, Name = SubstanceName } });

        await service.DeleteSubstanceAsync(SubstanceName);

        await substances.Received().DeleteAsync(ItemId);
    }

    [TestMethod]
    public async Task GetHighRiskMedicinesAsync_ReturnsRepositoryResult()
    {
        var (service, _, _, highRisk, _) = CreateService();
        highRisk.GetAllAsync().Returns(new List<HighRiskMedicine> { new() { Id = ItemId } });

        var result = await service.GetHighRiskMedicinesAsync();

        Assert.HasCount(1, result);
    }

    private static Item ValidItem() => new(MatchingItemName, Producer, Category, Price, Pills)
    {
        ItemSubstanceEntries = [new ItemSubstance { Substance = new Substance { Name = SubstanceName }, Concentration = 1f }],
    };

    [TestMethod]
    public void GetAllItems_ReturnsHydratedItems()
    {
        var (service, items, _, _, _) = CreateService();
        items.GetAllAsync().Returns(new List<Item> { new() { Id = ItemId, Name = MatchingItemName } });

        var result = service.GetAllItems();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void GetItemById_ReturnsItem()
    {
        var (service, items, _, _, _) = CreateService();
        items.GetByIdAsync(ItemId).Returns(new Item { Id = ItemId });

        var result = service.GetItemById(ItemId);

        Assert.AreEqual(ItemId, result!.Id);
    }

    [TestMethod]
    public void GetExpiredItems_ReturnsItemsWithExpiredBatches()
    {
        var (service, items, _, _, _) = CreateService();
        items.GetAllAsync().Returns(new List<Item>
        {
            new() { Id = ItemId, Name = MatchingItemName, ItemBatchEntries = [new ItemBatch { ExpirationDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), NumberOfPacks = 1 }] },
        });

        var result = service.GetExpiredItems();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void GetAllSubstances_ReturnsRepositoryResult()
    {
        var (service, _, substances, _, _) = CreateService();
        substances.GetAllAsync().Returns(new List<Substance> { new() { Name = SubstanceName } });

        var result = service.GetAllSubstances();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void GetSubstanceByName_ReturnsMatch()
    {
        var (service, _, substances, _, _) = CreateService();
        substances.GetAllAsync().Returns(new List<Substance> { new() { Name = SubstanceName } });

        var result = service.GetSubstanceByName(SubstanceName);

        Assert.AreEqual(SubstanceName, result!.Name);
    }

    [TestMethod]
    public void SubstanceExists_KnownSubstance_ReturnsTrue()
    {
        var (service, _, substances, _, _) = CreateService();
        substances.GetAllAsync().Returns(new List<Substance> { new() { Name = SubstanceName } });

        bool exists = service.SubstanceExists(SubstanceName);

        Assert.IsTrue(exists);
    }

    [TestMethod]
    public void GetTop30Items_ReturnsAggregatedTuples()
    {
        var (service, items, _, _, orders) = CreateService();
        var item = new Item { Id = ItemId, Name = MatchingItemName };
        items.GetAllAsync().Returns(new List<Item> { item });
        orders.GetAllAsync().Returns(new List<Order> { new() { Id = OrderId } });
        orders.GetOrderItemsByOrderIdAsync(OrderId).Returns(new List<OrderItem> { new() { Item = item, OrderQuantity = OrderQuantity } });

        var result = service.GetTop30Items();

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void GetTop30Substances_ReturnsCounts()
    {
        var (service, items, _, _, _) = CreateService();
        items.GetAllAsync().Returns(new List<Item> { ValidItem() });

        var result = service.GetTop30Substances();

        Assert.AreEqual(1, result[SubstanceName]);
    }

    [TestMethod]
    public void AddItem_Valid_CreatesItem()
    {
        var (service, items, _, _, _) = CreateService();
        items.GetAllAsync().Returns(new List<Item>());

        service.AddItem(ValidItem());

        items.Received().CreateAsync(Arg.Is<Item>(item => item.Name == MatchingItemName));
    }

    [TestMethod]
    public void AddSubstance_New_CreatesSubstance()
    {
        var (service, _, substances, _, _) = CreateService();
        substances.GetAllAsync().Returns(new List<Substance>());

        service.AddSubstance(new Substance { Name = SubstanceName, LethalDose = LethalDose });

        substances.Received().CreateAsync(Arg.Is<Substance>(substance => substance.Name == SubstanceName));
    }

    [TestMethod]
    public void RemoveItemById_DelegatesToRepository()
    {
        var (service, items, _, _, _) = CreateService();

        service.RemoveItemById(ItemId);

        items.Received().DeleteAsync(ItemId);
    }

    [TestMethod]
    public async Task GetItemsAsync_AllFiltersApplied_ReturnsMatchingItem()
    {
        var (service, items, _, _, _) = CreateService();
        items.GetAllAsync().Returns(new List<Item>
        {
            new(MatchingItemName, Producer, Category, Price, Pills, quantity: 5, discount: 10f)
            {
                ItemSubstanceEntries = [new ItemSubstance { Substance = new Substance { Name = SubstanceName }, Concentration = 1f }],
            },
        });

        var result = await service.GetItemsAsync(
            search: MatchingItemName,
            categories: [Category],
            priceRanges: [(0f, 100f)],
            stockFilter: Hospital.Shared.Services.IProductCatalogueService.StockFilterInStock,
            discounted: true,
            substances: [SubstanceName],
            ascending: true,
            sortBy: Hospital.Shared.Services.IProductCatalogueService.SortByPrice);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetItemsAsync_NewestDescendingNotDiscounted_ReturnsItems()
    {
        var (service, items, _, _, _) = CreateService();
        items.GetAllAsync().Returns(new List<Item> { new(MatchingItemName, Producer, Category, Price, Pills, quantity: 5) });

        var result = await service.GetItemsAsync(
            search: null,
            discounted: false,
            ascending: false,
            sortBy: Hospital.Shared.Services.IProductCatalogueService.SortByNewest);

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void UpdateItemById_Existing_UpdatesItem()
    {
        var (service, items, _, _, _) = CreateService();
        items.GetByIdNoTrackingAsync(ItemId).Returns(new Item { Id = ItemId });

        service.UpdateItemById(ItemId, new Item { Name = MatchingItemName });

        items.Received().UpdateWithEntriesAsync(Arg.Any<Item>());
    }

    [TestMethod]
    public void AddItemWithQuantity_Valid_CreatesItem()
    {
        var (service, items, _, _, _) = CreateService();
        items.GetAllAsync().Returns(new List<Item>());

        service.AddItemWithQuantity(ValidItem());

        items.Received().CreateAsync(Arg.Is<Item>(item => item.Name == MatchingItemName));
    }

    [TestMethod]
    public void UpdateSubstanceByName_Existing_UpdatesSubstance()
    {
        var (service, _, substances, _, _) = CreateService();
        substances.GetAllAsync().Returns(new List<Substance> { new() { Name = SubstanceName } });

        service.UpdateSubstanceByName(SubstanceName, new Substance { Name = SubstanceName });

        substances.Received().UpdateAsync(Arg.Any<Substance>());
    }

    [TestMethod]
    public void RemoveSubstanceByName_Existing_DeletesSubstance()
    {
        var (service, _, substances, _, _) = CreateService();
        substances.GetAllAsync().Returns(new List<Substance> { new() { Id = ItemId, Name = SubstanceName } });

        service.RemoveSubstanceByName(new Substance { Name = SubstanceName });

        substances.Received().DeleteAsync(ItemId);
    }

    [TestMethod]
    public void GetNotificationsForUser_ReturnsEmpty()
    {
        var (service, _, _, _, _) = CreateService();

        var result = service.GetNotificationsForUser(new User());

        Assert.IsEmpty(result);
    }
}
