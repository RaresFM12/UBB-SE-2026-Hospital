using Hospital.Data.Models;

namespace Hospital.Shared.Proxies
{
    public interface IAdminApiClient
    {
        void AddItem(Item item);
        void AddItemWithQuantity(Item item);
        void AddSubstance(Substance newSubstance);
        Task CreateItemAsync(string name, string producer, string category, float price, int numberOfPills, string label, string description, string imagePath, float discount, CancellationToken cancellationToken = default);
        Task CreateItemWithQuantityAsync(string name, string producer, string category, float price, int numberOfPills, int quantity, Dictionary<string, float> activeSubstances, Dictionary<DateOnly, int> batches, string label, string description, string imagePath, float discount, CancellationToken cancellationToken = default);
        Task CreateSubstanceAsync(string name, float lethalDose, string description, CancellationToken cancellationToken = default);
        Task DeleteItemAsync(int itemId, CancellationToken cancellationToken = default);
        Task DeleteSubstanceAsync(string name, CancellationToken cancellationToken = default);
        List<Item> GetAllItems();
        List<Substance> GetAllSubstances();
        List<Item> GetExpiredItems();
        Task<IReadOnlyList<HighRiskMedicine>> GetHighRiskMedicinesAsync(CancellationToken cancellationToken = default);
        Item? GetItemById(int itemId);
        Task<Item?> GetItemByIdAsync(int itemId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Item>> GetItemsAsync(string? name = null, CancellationToken cancellationToken = default);
        IReadOnlyList<Notification> GetNotificationsForUser(User user);
        Substance? GetSubstanceByName(string name);
        Task<Substance?> GetSubstanceByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Substance>> GetSubstancesAsync(CancellationToken cancellationToken = default);
        List<Tuple<int, string, int>> GetTop30Items();
        Dictionary<string, int> GetTop30Substances();
        Task<IReadOnlyList<(int ItemId, string ItemName, int OrderCount)>> GetTopItemsAsync(CancellationToken cancellationToken = default);
        Task<Dictionary<string, int>> GetTopSubstancesAsync(CancellationToken cancellationToken = default);
        Task<bool> ItemExistsAsync(int itemId, CancellationToken cancellationToken = default);
        void RemoveItemById(int itemId);
        void RemoveSubstanceByName(Substance substance);
        List<Item> SearchItemsByName(string query);
        Notification SendAboutToExpireNotification();
        Notification SendNewStockNotification(Item item);
        bool SubstanceExists(string name);
        Task<bool> SubstanceExistsAsync(string name, CancellationToken cancellationToken = default);
        Task UpdateItemAsync(Item item, CancellationToken cancellationToken = default);
        void UpdateItemById(int itemId, Item updatedItem);
        Task UpdateSubstanceAsync(Substance substance, CancellationToken cancellationToken = default);
        void UpdateSubstanceByName(string name, Substance substance);
    }
}