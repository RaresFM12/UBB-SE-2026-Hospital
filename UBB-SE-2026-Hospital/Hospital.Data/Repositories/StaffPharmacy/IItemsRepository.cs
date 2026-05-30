using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface IItemsRepository
{
    Task<Item?> GetByIdAsync(int itemId);
    Task<List<Item>> GetAllAsync();
    Task<List<Item>> GetLowStockItemsAsync(int threshold);
    Task<Item> CreateAsync(Item item);
    Task<Item> UpdateAsync(Item item);
    Task DeleteAsync(int itemId);

    Task<List<ItemBatch>> GetBatchesByItemIdAsync(int itemId);
    Task<ItemBatch> AddBatchAsync(ItemBatch batch);
    Task<ItemBatch> UpdateBatchAsync(ItemBatch batch);
    Task DeleteBatchAsync(int batchId);

    Task<List<Substance>> GetAllSubstancesAsync();
    Task<List<ItemSubstance>> GetSubstancesByItemIdAsync(int itemId);
}
