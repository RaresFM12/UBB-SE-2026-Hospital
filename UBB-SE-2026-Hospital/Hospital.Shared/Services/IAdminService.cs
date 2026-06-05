using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IAdminService
{
    Task<IReadOnlyList<Item>> GetItemsAsync(string? name = null, CancellationToken cancellationToken = default);
    List<Item> GetAllItems();
    List<Item> SearchItemsByName(string name);
    List<Item> GetExpiredItems();

    Task<Item?> GetItemByIdAsync(int itemId, CancellationToken cancellationToken = default);
    Item? GetItemById(int itemId);

    Task<bool> ItemExistsAsync(int itemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(int ItemId, string ItemName, int OrderCount)>> GetTopItemsAsync(CancellationToken cancellationToken = default);
    List<Tuple<int, string, int>> GetTop30Items();

    Task CreateItemAsync(string name, string producer, string category, float price, int numberOfPills, string label, string description, string imagePath, float discount, CancellationToken cancellationToken = default);
    void AddItem(Item item);

    Task CreateItemWithQuantityAsync(string name, string producer, string category, float price, int numberOfPills, int quantity, Dictionary<string, float> activeSubstances, Dictionary<DateOnly, int> batches, string label, string description, string imagePath, float discount, CancellationToken cancellationToken = default);

    Task UpdateItemAsync(Item item, CancellationToken cancellationToken = default);
    void UpdateItemById(int itemId, Item item);

    Task DeleteItemAsync(int itemId, CancellationToken cancellationToken = default);
    void RemoveItemById(int itemId);

    Task<IReadOnlyList<Substance>> GetSubstancesAsync(CancellationToken cancellationToken = default);
    List<Substance> GetAllSubstances();

    Task<Substance?> GetSubstanceByNameAsync(string name, CancellationToken cancellationToken = default);
    Substance? GetSubstanceByName(string name);

    Task<bool> SubstanceExistsAsync(string name, CancellationToken cancellationToken = default);

    Task<Dictionary<string, int>> GetTopSubstancesAsync(CancellationToken cancellationToken = default);
    Dictionary<string, int> GetTop30Substances();

    Task CreateSubstanceAsync(string name, float lethalDose, string description, CancellationToken cancellationToken = default);
    void AddSubstance(Substance substance);

    Task UpdateSubstanceAsync(Substance substance, CancellationToken cancellationToken = default);
    void UpdateSubstanceByName(string name, Substance substance);

    Task DeleteSubstanceAsync(string name, CancellationToken cancellationToken = default);
    void RemoveSubstanceByName(Substance substance);

    Task<IReadOnlyList<HighRiskMedicine>> GetHighRiskMedicinesAsync(CancellationToken cancellationToken = default);
    IReadOnlyList<Notification> GetNotificationsForUser(User user);
}
