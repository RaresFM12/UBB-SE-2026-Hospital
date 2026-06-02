using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class AdminService(
    IItemsRepository itemsRepository,
    ISubstancesRepository substancesRepository,
    IHighRiskMedicineRepository highRiskMedicineRepository,
    IOrdersRepository ordersRepository) : IAdminService
{
    private const int MinimumPositiveValue = 1;

    public async Task<IReadOnlyList<Item>> GetItemsAsync(string? name = null, CancellationToken cancellationToken = default)
    {
        var items = await itemsRepository.GetAllAsync();
        foreach (var item in items)
        {
            HydrateItem(item);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return items;
        }

        return items
            .Where(item => item.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<Item?> GetItemByIdAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var item = await itemsRepository.GetByIdAsync(itemId);
        if (item is not null)
        {
            HydrateItem(item);
        }

        return item;
    }

    public async Task<bool> ItemExistsAsync(int itemId, CancellationToken cancellationToken = default)
        => await itemsRepository.GetByIdAsync(itemId) is not null;

    public async Task<IReadOnlyList<(int ItemId, string ItemName, int OrderCount)>> GetTopItemsAsync(CancellationToken cancellationToken = default)
    {
        var itemsById = (await itemsRepository.GetAllAsync()).ToDictionary(item => item.Id);
        var allOrders = await ordersRepository.GetAllAsync();
        var counts = new Dictionary<int, int>();

        foreach (var order in allOrders)
        {
            var orderItems = await ordersRepository.GetOrderItemsByOrderIdAsync(order.Id);
            foreach (var orderItem in orderItems)
            {
                if (orderItem.Item is null)
                {
                    continue;
                }

                counts.TryGetValue(orderItem.Item.Id, out int count);
                counts[orderItem.Item.Id] = count + orderItem.OrderQuantity;
            }
        }

        return counts
            .OrderByDescending(pair => pair.Value)
            .Take(30)
            .Select(pair => (pair.Key, itemsById.GetValueOrDefault(pair.Key)?.Name ?? $"Item #{pair.Key}", pair.Value))
            .ToList();
    }

    public async Task CreateItemAsync(string name, string producer, string category, float price, int numberOfPills, string label, string description, string imagePath, float discount, CancellationToken cancellationToken = default)
    {
        var item = new Item(name, producer, category, price, numberOfPills, label: label, description: description, imagePath: imagePath, discount: discount);
        await ValidateItemForAddAsync(item);
        await itemsRepository.CreateAsync(item);
    }

    public async Task CreateItemWithQuantityAsync(string name, string producer, string category, float price, int numberOfPills, int quantity, Dictionary<string, float> activeSubstances, Dictionary<DateOnly, int> batches, string label, string description, string imagePath, float discount, CancellationToken cancellationToken = default)
    {
        var item = new Item(name, producer, category, price, numberOfPills, quantity, label, description, imagePath, discount)
        {
            ActiveSubstances = activeSubstances,
            Batches = batches,
        };

        await ValidateItemForAddAsync(item);
        BuildItemEntries(item);
        await itemsRepository.CreateAsync(item);
    }

    public async Task UpdateItemAsync(Item item, CancellationToken cancellationToken = default)
    {
        var existing = await itemsRepository.GetByIdAsync(item.Id)
            ?? throw new ArgumentException("Item with the specified ID does not exist.");

        HydrateItem(existing);
        BuildItemEntries(item);
        await SyncItemEntriesAsync(existing, item);
        await itemsRepository.UpdateAsync(item);
    }

    public async Task DeleteItemAsync(int itemId, CancellationToken cancellationToken = default)
        => await itemsRepository.DeleteAsync(itemId);

    public async Task<IReadOnlyList<Substance>> GetSubstancesAsync(CancellationToken cancellationToken = default)
        => await substancesRepository.GetAllAsync();

    public async Task<Substance?> GetSubstanceByNameAsync(string name, CancellationToken cancellationToken = default)
        => (await substancesRepository.GetAllAsync())
            .FirstOrDefault(substance => string.Equals(substance.Name, name, StringComparison.OrdinalIgnoreCase));

    public async Task<bool> SubstanceExistsAsync(string name, CancellationToken cancellationToken = default)
        => await GetSubstanceByNameAsync(name, cancellationToken) is not null;

    public async Task<Dictionary<string, int>> GetTopSubstancesAsync(CancellationToken cancellationToken = default)
    {
        var items = await itemsRepository.GetAllAsync();
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in items)
        {
            HydrateItem(item);
            foreach (var substanceName in item.ActiveSubstances.Keys)
            {
                counts.TryGetValue(substanceName, out int count);
                counts[substanceName] = count + 1;
            }
        }

        return counts
            .OrderByDescending(pair => pair.Value)
            .Take(30)
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
    }

    public async Task CreateSubstanceAsync(string name, float lethalDose, string description, CancellationToken cancellationToken = default)
    {
        if (await SubstanceExistsAsync(name, cancellationToken))
        {
            throw new ArgumentException($"Substance {name} exists already.");
        }

        await substancesRepository.CreateAsync(new Substance
        {
            Name = name,
            LethalDose = lethalDose,
            Description = description,
        });
    }

    public async Task UpdateSubstanceAsync(Substance substance, CancellationToken cancellationToken = default)
    {
        if (!await SubstanceExistsAsync(substance.Name, cancellationToken))
        {
            throw new ArgumentException($"Substance {substance.Name} does NOT exist.");
        }

        await substancesRepository.UpdateAsync(substance);
    }

    public async Task DeleteSubstanceAsync(string name, CancellationToken cancellationToken = default)
    {
        var substance = await GetSubstanceByNameAsync(name, cancellationToken)
            ?? throw new ArgumentException($"Substance {name} does NOT exist.");

        await substancesRepository.DeleteAsync(substance.Id);
    }

    public async Task<IReadOnlyList<HighRiskMedicine>> GetHighRiskMedicinesAsync(CancellationToken cancellationToken = default)
        => await highRiskMedicineRepository.GetAllAsync();

    private async Task ValidateItemForAddAsync(Item item)
    {
        if (string.IsNullOrWhiteSpace(item.Name) ||
            string.IsNullOrWhiteSpace(item.Producer) ||
            item.Price < MinimumPositiveValue ||
            item.NumberOfPills < MinimumPositiveValue ||
            item.Quantity < 0 ||
            item.DiscountPercentage < 0)
        {
            throw new ArgumentException("Invalid item data. Please check the input and try again.");
        }

        var existingItems = await itemsRepository.GetAllAsync();
        if (existingItems.Any(existingItem =>
                existingItem.Id != item.Id &&
                string.Equals(existingItem.Name, item.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException($"An item named \"{item.Name}\" already exists.");
        }
    }

    private static void HydrateItem(Item item)
    {
        item.ActiveSubstances = item.ItemSubstanceEntries
            .Where(entry => entry.Substance is not null)
            .ToDictionary(entry => entry.Substance.Name, entry => entry.Concentration, StringComparer.OrdinalIgnoreCase);

        item.Batches = item.ItemBatchEntries
            .ToDictionary(entry => entry.ExpirationDate, entry => entry.NumberOfPacks);
    }

    private void BuildItemEntries(Item item)
    {
        item.ItemSubstanceEntries = item.ActiveSubstances
            .Select(pair => new ItemSubstance
            {
                Item = item,
                Substance = new Substance { Name = pair.Key },
                Concentration = pair.Value,
            })
            .ToList();

        item.ItemBatchEntries = item.Batches
            .Select(pair => new ItemBatch
            {
                Item = item,
                ExpirationDate = pair.Key,
                NumberOfPacks = pair.Value,
            })
            .ToList();
    }

    private async Task SyncItemEntriesAsync(Item existing, Item updated)
    {
        var substanceLookup = (await substancesRepository.GetAllAsync())
            .ToDictionary(substance => substance.Name, StringComparer.OrdinalIgnoreCase);

        updated.ItemSubstanceEntries = updated.ActiveSubstances
            .Select(pair => new ItemSubstance
            {
                Id = existing.ItemSubstanceEntries
                    .FirstOrDefault(entry => string.Equals(entry.Substance?.Name, pair.Key, StringComparison.OrdinalIgnoreCase))?.Id ?? 0,
                Item = updated,
                Substance = substanceLookup.TryGetValue(pair.Key, out var existingSubstance)
                    ? existingSubstance
                    : new Substance { Name = pair.Key },
                Concentration = pair.Value,
            })
            .ToList();

        updated.ItemBatchEntries = updated.Batches
            .Select(pair => new ItemBatch
            {
                Id = existing.ItemBatchEntries
                    .FirstOrDefault(entry => entry.ExpirationDate == pair.Key)?.Id ?? 0,
                Item = updated,
                ExpirationDate = pair.Key,
                NumberOfPacks = pair.Value,
            })
            .ToList();
    }
}
