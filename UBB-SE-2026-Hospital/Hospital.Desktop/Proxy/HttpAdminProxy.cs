using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Proxy;

public class HttpAdminProxy(HttpClient httpClient) : ProxyBase(httpClient), IAdminService
{
    private const string ItemsUri = "api/items";
    private const string SubstancesUri = "api/substances";
    private const string HighRiskUri = "api/high-risk-medicines";

    // --- Items ---

    public async Task<IReadOnlyList<Item>> GetItemsAsync(string? name = null, CancellationToken cancellationToken = default)
        => await GetAsync<List<Item>>(name is null ? ItemsUri : $"{ItemsUri}?name={Uri.EscapeDataString(name)}") ?? [];

    public List<Item> GetAllItems()
        => Task.Run(async () => await GetAsync<List<Item>>(ItemsUri) ?? []).GetAwaiter().GetResult();

    public List<Item> SearchItemsByName(string query)
        => Task.Run(async () => await GetAsync<List<Item>>($"{ItemsUri}?name={Uri.EscapeDataString(query)}") ?? []).GetAwaiter().GetResult();

    public List<Item> GetExpiredItems()
        => Task.Run(async () => await GetAsync<List<Item>>($"{ItemsUri}/expired") ?? []).GetAwaiter().GetResult();

    public async Task<Item?> GetItemByIdAsync(int itemId, CancellationToken cancellationToken = default)
        => await GetAsync<Item>($"{ItemsUri}/{itemId}");

    public Item? GetItemById(int itemId)
        => Task.Run(async () => await GetAsync<Item>($"{ItemsUri}/{itemId}")).GetAwaiter().GetResult();

    public async Task<bool> ItemExistsAsync(int itemId, CancellationToken cancellationToken = default)
        => await GetAsync<bool>($"{ItemsUri}/{itemId}/exists");

    public async Task<IReadOnlyList<(int ItemId, string ItemName, int OrderCount)>> GetTopItemsAsync(CancellationToken cancellationToken = default)
    {
        var raw = await GetAsync<List<TopItemDto>>($"{ItemsUri}/top") ?? [];
        return raw.Select(x => (x.ItemId, x.ItemName, x.OrderCount)).ToList();
    }

    public List<Tuple<int, string, int>> GetTop30Items()
        => Task.Run(async () => await GetAsync<List<Tuple<int, string, int>>>($"{ItemsUri}/top") ?? []).GetAwaiter().GetResult();

    public async Task CreateItemAsync(string name, string producer, string category, float price, int numberOfPills, string label, string description, string imagePath, float discount, CancellationToken cancellationToken = default)
        => await PostAsync<object, object>(ItemsUri, new { name, producer, category, price, numberOfPills, label, description, imagePath, discount });

    public void AddItem(Item item)
        => Task.Run(async () => await PostAsync<Item, object>(ItemsUri, item)).GetAwaiter().GetResult();

    public async Task CreateItemWithQuantityAsync(string name, string producer, string category, float price, int numberOfPills, int quantity, Dictionary<string, float> activeSubstances, Dictionary<DateOnly, int> batches, string label, string description, string imagePath, float discount, CancellationToken cancellationToken = default)
        => await PostAsync<object, object>($"{ItemsUri}/with-quantity", new { name, producer, category, price, numberOfPills, quantity, activeSubstances, batches, label, description, imagePath, discount });

    public async Task UpdateItemAsync(Item item, CancellationToken cancellationToken = default)
        => await PutAsync($"{ItemsUri}/{item.Id}", item);

    public void UpdateItemById(int itemId, Item updatedItem)
        => Task.Run(async () => await PutAsync($"{ItemsUri}/{itemId}", updatedItem)).GetAwaiter().GetResult();

    public async Task DeleteItemAsync(int itemId, CancellationToken cancellationToken = default)
        => await DeleteAsync($"{ItemsUri}/{itemId}");

    public void RemoveItemById(int itemId)
        => Task.Run(async () => await DeleteAsync($"{ItemsUri}/{itemId}")).GetAwaiter().GetResult();

    // --- Substances ---

    public async Task<IReadOnlyList<Substance>> GetSubstancesAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<Substance>>(SubstancesUri) ?? [];

    public List<Substance> GetAllSubstances()
        => Task.Run(async () => await GetAsync<List<Substance>>(SubstancesUri) ?? []).GetAwaiter().GetResult();

    public async Task<Substance?> GetSubstanceByNameAsync(string name, CancellationToken cancellationToken = default)
        => await GetAsync<Substance>($"{SubstancesUri}/{Uri.EscapeDataString(name)}");

    public Substance? GetSubstanceByName(string name)
        => Task.Run(async () => await GetAsync<Substance>($"{SubstancesUri}/{Uri.EscapeDataString(name)}")).GetAwaiter().GetResult();

    public async Task<bool> SubstanceExistsAsync(string name, CancellationToken cancellationToken = default)
        => await GetAsync<bool>($"{SubstancesUri}/{Uri.EscapeDataString(name)}/exists");

    public bool SubstanceExists(string name)
        => Task.Run(async () => await GetAsync<bool>($"{SubstancesUri}/{Uri.EscapeDataString(name)}/exists")).GetAwaiter().GetResult();

    public async Task<Dictionary<string, int>> GetTopSubstancesAsync(CancellationToken cancellationToken = default)
        => await GetAsync<Dictionary<string, int>>($"{SubstancesUri}/top") ?? [];

    public Dictionary<string, int> GetTop30Substances()
        => Task.Run(async () => await GetAsync<Dictionary<string, int>>($"{SubstancesUri}/top") ?? []).GetAwaiter().GetResult();

    public async Task CreateSubstanceAsync(string name, float lethalDose, string description, CancellationToken cancellationToken = default)
        => await PostAsync<object, object>(SubstancesUri, new { name, lethalDose, description });

    public void AddSubstance(Substance newSubstance)
        => Task.Run(async () => await PostAsync<Substance, object>(SubstancesUri, newSubstance)).GetAwaiter().GetResult();

    public async Task UpdateSubstanceAsync(Substance substance, CancellationToken cancellationToken = default)
        => await PutAsync($"{SubstancesUri}/{Uri.EscapeDataString(substance.Name)}", substance);

    public void UpdateSubstanceByName(string name, Substance substance)
        => Task.Run(async () => await PutAsync($"{SubstancesUri}/{Uri.EscapeDataString(name)}", substance)).GetAwaiter().GetResult();

    public async Task DeleteSubstanceAsync(string name, CancellationToken cancellationToken = default)
        => await DeleteAsync($"{SubstancesUri}/{Uri.EscapeDataString(name)}");

    public void RemoveSubstanceByName(Substance substance)
        => Task.Run(async () => await DeleteAsync($"{SubstancesUri}/{Uri.EscapeDataString(substance.Name)}")).GetAwaiter().GetResult();

    // --- High risk & notifications ---

    public async Task<IReadOnlyList<HighRiskMedicine>> GetHighRiskMedicinesAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<HighRiskMedicine>>(HighRiskUri) ?? [];

    public IReadOnlyList<Notification> GetNotificationsForUser(User user)
        => [];

    public Notification SendNewStockNotification(Item item)
        => new Notification { Title = "Stock Alert", Message = $"Item {item.Name} is running low." };

    public Notification SendAboutToExpireNotification()
        => new Notification { Title = "Expiry Alert", Message = "Some items are about to expire." };

    private sealed class TopItemDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
    }
    public void AddItemWithQuantity(Item item)
        => Task.Run(async () => await PostAsync<Item, object>($"{ItemsUri}/with-quantity", item)).GetAwaiter().GetResult();

}
