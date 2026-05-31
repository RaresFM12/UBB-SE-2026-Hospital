using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class AdminService : IAdminService
{
    public Task<IReadOnlyList<Item>> GetItemsAsync(string? name = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<Item?> GetItemByIdAsync(int itemId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> ItemExistsAsync(int itemId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<(int ItemId, string ItemName, int OrderCount)>> GetTopItemsAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task CreateItemAsync(string name, string producer, string category, float price, int numberOfPills, string label, string description, string imagePath, float discount, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task CreateItemWithQuantityAsync(string name, string producer, string category, float price, int numberOfPills, int quantity, Dictionary<string, float> activeSubstances, Dictionary<DateOnly, int> batches, string label, string description, string imagePath, float discount, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task UpdateItemAsync(Item item, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task DeleteItemAsync(int itemId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<Substance>> GetSubstancesAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<Substance?> GetSubstanceByNameAsync(string name, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> SubstanceExistsAsync(string name, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<Dictionary<string, int>> GetTopSubstancesAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task CreateSubstanceAsync(string name, float lethalDose, string description, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task UpdateSubstanceAsync(Substance substance, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task DeleteSubstanceAsync(string name, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<HighRiskMedicine>> GetHighRiskMedicinesAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
