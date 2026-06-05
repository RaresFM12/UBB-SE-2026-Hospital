using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class ItemsRepository(HospitalDbContext context) : IItemsRepository
{
    public async Task<Item?> GetByIdAsync(int itemId)
        => await context.Items
            .Include(i => i.ItemBatchEntries)
            .Include(i => i.ItemSubstanceEntries)
                .ThenInclude(s => s.Substance)
            .FirstOrDefaultAsync(i => i.Id == itemId);

    public async Task<Item?> GetByIdNoTrackingAsync(int itemId)
        => await context.Items
            .AsNoTracking()
            .Include(i => i.ItemBatchEntries)
            .Include(i => i.ItemSubstanceEntries)
                .ThenInclude(s => s.Substance)
            .FirstOrDefaultAsync(i => i.Id == itemId);

    public async Task<List<Item>> GetAllAsync()
        => await context.Items
            .Include(i => i.ItemBatchEntries)
            .Include(i => i.ItemSubstanceEntries)
                .ThenInclude(s => s.Substance)
            .ToListAsync();

    public async Task<List<Item>> GetLowStockItemsAsync(int threshold)
        => await context.Items
            .Where(i => i.Quantity <= threshold)
            .ToListAsync();

    public async Task<Item> CreateAsync(Item item)
    {
        context.Items.Add(item);
        await context.SaveChangesAsync();
        return item;
    }

    public async Task<Item> UpdateAsync(Item item)
    {
        context.Items.Update(item);
        await context.SaveChangesAsync();
        return item;
    }

    public async Task UpdateWithEntriesAsync(Item updatedItem)
    {
        var existingItem = await context.Items
            .Include(item => item.ItemSubstanceEntries)
            .Include(item => item.ItemBatchEntries)
            .FirstOrDefaultAsync(item => item.Id == updatedItem.Id);

        if (existingItem is null)
        {
            return;
        }

        existingItem.Name = updatedItem.Name;
        existingItem.Price = updatedItem.Price;
        existingItem.Category = updatedItem.Category;
        existingItem.NumberOfPills = updatedItem.NumberOfPills;
        existingItem.Producer = updatedItem.Producer;
        existingItem.ImagePath = updatedItem.ImagePath;
        existingItem.Label = updatedItem.Label;
        existingItem.Description = updatedItem.Description;
        existingItem.DiscountPercentage = updatedItem.DiscountPercentage;
        existingItem.Quantity = updatedItem.Quantity;

        context.ItemSubstances.RemoveRange(existingItem.ItemSubstanceEntries);
        existingItem.ItemSubstanceEntries.Clear();
        foreach (var pair in updatedItem.ActiveSubstances)
        {
            var substance = await context.Substances
                .FirstOrDefaultAsync(substance => substance.Name == pair.Key);
            if (substance is null)
            {
                continue;
            }

            existingItem.ItemSubstanceEntries.Add(new ItemSubstance
            {
                Substance = substance,
                Concentration = pair.Value,
            });
        }

        context.ItemBatches.RemoveRange(existingItem.ItemBatchEntries);
        existingItem.ItemBatchEntries.Clear();
        foreach (var pair in updatedItem.Batches)
        {
            existingItem.ItemBatchEntries.Add(new ItemBatch
            {
                ExpirationDate = pair.Key,
                NumberOfPacks = pair.Value,
            });
        }

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int itemId)
    {
        var item = await context.Items.FindAsync(itemId);
        if (item is not null)
        {
            context.Items.Remove(item);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<ItemBatch>> GetBatchesByItemIdAsync(int itemId)
        => await context.ItemBatches.Where(b => b.Item.Id == itemId).ToListAsync();

    public async Task<ItemBatch> AddBatchAsync(ItemBatch batch)
    {
        context.ItemBatches.Add(batch);
        await context.SaveChangesAsync();
        return batch;
    }

    public async Task<ItemBatch> UpdateBatchAsync(ItemBatch batch)
    {
        context.ItemBatches.Update(batch);
        await context.SaveChangesAsync();
        return batch;
    }

    public async Task DeleteBatchAsync(int batchId)
    {
        var batch = await context.ItemBatches.FindAsync(batchId);
        if (batch is not null)
        {
            context.ItemBatches.Remove(batch);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<Substance>> GetAllSubstancesAsync()
        => await context.Substances.ToListAsync();

    public async Task<List<ItemSubstance>> GetSubstancesByItemIdAsync(int itemId)
        => await context.ItemSubstances
            .Include(s => s.Substance)
            .Where(s => s.Item.Id == itemId)
            .ToListAsync();
}
