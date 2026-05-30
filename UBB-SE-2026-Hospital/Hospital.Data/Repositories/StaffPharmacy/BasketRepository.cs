using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data.Repositories;

public class BasketRepository(HospitalDbContext context) : IBasketRepository
{
    public async Task<List<BasketEntry>> GetBasketByUserIdAsync(int userId)
        => await context.BasketEntries.Where(b => b.UserId == userId).ToListAsync();

    public async Task<BasketEntry?> GetBasketEntryAsync(int userId, int itemId)
        => await context.BasketEntries
            .FirstOrDefaultAsync(b => b.UserId == userId && b.ItemId == itemId);

    public async Task<BasketEntry> AddToBasketAsync(BasketEntry entry)
    {
        context.BasketEntries.Add(entry);
        await context.SaveChangesAsync();
        return entry;
    }

    public async Task<BasketEntry> UpdateBasketEntryAsync(BasketEntry entry)
    {
        context.BasketEntries.Update(entry);
        await context.SaveChangesAsync();
        return entry;
    }

    public async Task RemoveFromBasketAsync(int userId, int itemId)
    {
        var entry = await GetBasketEntryAsync(userId, itemId);
        if (entry is not null)
        {
            context.BasketEntries.Remove(entry);
            await context.SaveChangesAsync();
        }
    }

    public async Task ClearBasketAsync(int userId)
    {
        var entries = await context.BasketEntries.Where(b => b.UserId == userId).ToListAsync();
        context.BasketEntries.RemoveRange(entries);
        await context.SaveChangesAsync();
    }
}
