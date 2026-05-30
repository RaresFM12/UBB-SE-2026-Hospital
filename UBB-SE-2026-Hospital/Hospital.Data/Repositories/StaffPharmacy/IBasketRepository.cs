using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface IBasketRepository
{
    Task<List<BasketEntry>> GetBasketByUserIdAsync(int userId);
    Task<BasketEntry?> GetBasketEntryAsync(int userId, int itemId);
    Task<BasketEntry> AddToBasketAsync(BasketEntry entry);
    Task<BasketEntry> UpdateBasketEntryAsync(BasketEntry entry);
    Task RemoveFromBasketAsync(int userId, int itemId);
    Task ClearBasketAsync(int userId);
}
