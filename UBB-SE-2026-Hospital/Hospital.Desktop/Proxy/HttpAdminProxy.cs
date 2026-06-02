using Hospital.Shared.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Proxy;

public class HttpAdminProxy(HttpClient httpClient) : ProxyBase(httpClient), IAdminService
{
    private const string ItemsUri = "api/items";
    private const string SubstancesUri = "api/substances";
    private const string HighRiskUri = "api/high-risk-medicines";

    public List<Item> GetAllItems()
        => Task.Run(async () => await GetAsync<List<Item>>(ItemsUri) ?? []).GetAwaiter().GetResult();

    public List<Substance> GetAllSubstances()
        => Task.Run(async () => await GetAsync<List<Substance>>(SubstancesUri) ?? []).GetAwaiter().GetResult();

    public List<Item> SearchItemsByName(string query)
        => Task.Run(async () => await GetAsync<List<Item>>($"{ItemsUri}?name={Uri.EscapeDataString(query)}") ?? []).GetAwaiter().GetResult();

    public Item GetItemById(int itemId)
        => Task.Run(async () => await GetAsync<Item>($"{ItemsUri}/{itemId}")).GetAwaiter().GetResult()!;

    public Substance GetSubstanceByName(string name)
        => Task.Run(async () => await GetAsync<Substance>($"{SubstancesUri}/{Uri.EscapeDataString(name)}")).GetAwaiter().GetResult()!;

    public bool SubstanceExists(string name)
        => Task.Run(async () => await GetAsync<bool>($"{SubstancesUri}/{Uri.EscapeDataString(name)}/exists")).GetAwaiter().GetResult();

    public void AddItem(Item newItem)
        => Task.Run(async () => await PostAsync<Item, object>(ItemsUri, newItem)).GetAwaiter().GetResult();

    public void AddItemWithQuantity(Item newItem)
        => Task.Run(async () => await PostAsync<Item, object>($"{ItemsUri}/with-quantity", newItem)).GetAwaiter().GetResult();

    public void RemoveItemById(int itemId)
        => Task.Run(async () => await DeleteAsync($"{ItemsUri}/{itemId}")).GetAwaiter().GetResult();

    public void UpdateItemById(int itemId, Item updatedItem)
        => Task.Run(async () => await PutAsync($"{ItemsUri}/{itemId}", updatedItem)).GetAwaiter().GetResult();

    public void AddSubstance(Substance newSubstance)
        => Task.Run(async () => await PostAsync<Substance, object>(SubstancesUri, newSubstance)).GetAwaiter().GetResult();

    public void RemoveSubstanceByName(Substance substance)
        => Task.Run(async () => await DeleteAsync($"{SubstancesUri}/{Uri.EscapeDataString(substance.Name)}")).GetAwaiter().GetResult();

    public void UpdateSubstanceByName(string name, Substance substance)
        => Task.Run(async () => await PutAsync($"{SubstancesUri}/{Uri.EscapeDataString(name)}", substance)).GetAwaiter().GetResult();

    public void ValidateItemForAdd(Item item)
    {
    }

    public List<Item> GetExpiredItems()
        => Task.Run(async () => await GetAsync<List<Item>>($"{ItemsUri}/expired") ?? []).GetAwaiter().GetResult();

    public Notification SendNewStockNotification(Item item)
        => new Notification { Title = "Stock Alert", Message = $"Item {item.Name} is running low." };

    public Notification SendAboutToExpireNotification()
        => new Notification { Title = "Expiry Alert", Message = "Some items are about to expire." };

    public List<Notification> GetNotificationsForUser(User user)
        => [];

    public List<Tuple<int, string, int>> GetTop30Items()
        => Task.Run(async () => await GetAsync<List<Tuple<int, string, int>>>($"{ItemsUri}/top") ?? []).GetAwaiter().GetResult();

    public Dictionary<string, int> GetTop30Substances()
        => Task.Run(async () => await GetAsync<Dictionary<string, int>>($"{SubstancesUri}/top") ?? []).GetAwaiter().GetResult();
}
