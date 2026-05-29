using Hospital.Shared.Models.StaffPharmacy;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class AdminService : IAdminService
{
    public Task<IReadOnlyList<Item>> GetItemsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Item>>(
        [
            new Item
            {
                Id = 1,
                Name = "Merge placeholder item",
                Category = "Pharmacy",
                Producer = "Team 923-2",
                Price = 0,
                Quantity = 0,
            },
        ]);
}
