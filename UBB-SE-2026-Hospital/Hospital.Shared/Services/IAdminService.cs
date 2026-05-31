using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IAdminService
{
    // Items
    Task<IReadOnlyList<Item>> GetItemsAsync(string? name = null, CancellationToken cancellationToken = default);

    Task<Item?> GetItemByIdAsync(int itemId, CancellationToken cancellationToken = default);

    Task<bool> ItemExistsAsync(int itemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(int ItemId, string ItemName, int OrderCount)>> GetTopItemsAsync(CancellationToken cancellationToken = default);

    Task CreateItemAsync(string name, string producer, string category, float price, int numberOfPills, string label, string description, string imagePath, float discount, CancellationToken cancellationToken = default);

    Task CreateItemWithQuantityAsync(string name, string producer, string category, float price, int numberOfPills, int quantity, Dictionary<string, float> activeSubstances, Dictionary<DateOnly, int> batches, string label, string description, string imagePath, float discount, CancellationToken cancellationToken = default);

    Task UpdateItemAsync(Item item, CancellationToken cancellationToken = default);

    Task DeleteItemAsync(int itemId, CancellationToken cancellationToken = default);

    // Substances
    Task<IReadOnlyList<Substance>> GetSubstancesAsync(CancellationToken cancellationToken = default);

    Task<Substance?> GetSubstanceByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<bool> SubstanceExistsAsync(string name, CancellationToken cancellationToken = default);

    Task<Dictionary<string, int>> GetTopSubstancesAsync(CancellationToken cancellationToken = default);

    Task CreateSubstanceAsync(string name, float lethalDose, string description, CancellationToken cancellationToken = default);

    Task UpdateSubstanceAsync(Substance substance, CancellationToken cancellationToken = default);

    Task DeleteSubstanceAsync(string name, CancellationToken cancellationToken = default);

    // HighRiskMedicines
    Task<IReadOnlyList<HighRiskMedicine>> GetHighRiskMedicinesAsync(CancellationToken cancellationToken = default);
}
