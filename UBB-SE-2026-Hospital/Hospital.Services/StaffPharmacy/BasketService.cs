using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Models.StaffPharmacy;
using Hospital.Shared.Services;
using SharedBasketEntry = Hospital.Shared.Models.StaffPharmacy.BasketEntry;

namespace Hospital.Services.StaffPharmacy;

public class BasketService(
    IBasketRepository basketRepository,
    IUsersRepository usersRepository,
    IItemsRepository itemsRepository) : IBasketService
{
    public async Task<Dictionary<int, SharedBasketEntry>> GetBasketAsync(int userId, CancellationToken cancellationToken = default)
        => (await basketRepository.GetBasketByUserIdAsync(userId))
            .Where(entry => entry.Item is not null)
            .ToDictionary(
                entry => entry.Item.Id,
                entry => new SharedBasketEntry
                {
                    ItemId = entry.Item.Id,
                    Quantity = entry.Quantity,
                    ExtraDiscountPercentage = entry.ExtraDiscountPercentage,
                });

    public async Task SaveBasketAsync(int userId, Dictionary<int, SharedBasketEntry> basket, CancellationToken cancellationToken = default)
    {
        await basketRepository.ClearBasketAsync(userId);
        foreach (var entry in basket.Values)
        {
            await AddOrUpdateEntryAsync(userId, entry.ItemId, entry.Quantity, entry.ExtraDiscountPercentage);
        }
    }

    public async Task ClearBasketAsync(int userId, CancellationToken cancellationToken = default)
        => await basketRepository.ClearBasketAsync(userId);

    public async Task AddToBasketAsync(int userId, int itemId, int quantity, float extraDiscountPercentage = 0f, CancellationToken cancellationToken = default)
    {
        var existing = await basketRepository.GetBasketEntryAsync(userId, itemId);
        if (existing is not null)
        {
            existing.Quantity += quantity;
            if (extraDiscountPercentage > existing.ExtraDiscountPercentage)
            {
                existing.ExtraDiscountPercentage = extraDiscountPercentage;
            }

            await basketRepository.UpdateBasketEntryAsync(existing);
            return;
        }

        await AddOrUpdateEntryAsync(userId, itemId, quantity, extraDiscountPercentage);
    }

    private async Task AddOrUpdateEntryAsync(int userId, int itemId, int quantity, float extraDiscountPercentage)
    {
        var user = await usersRepository.GetUserByIdAsync(userId)
            ?? throw new ArgumentException($"User #{userId} not found.");
        var item = await itemsRepository.GetByIdAsync(itemId)
            ?? throw new ArgumentException($"Item #{itemId} not found.");

        await basketRepository.AddToBasketAsync(new Hospital.Data.Models.BasketEntry(quantity, extraDiscountPercentage)
        {
            User = user,
            Item = item,
        });
    }
}
