using Hospital.Shared.Models.StaffPharmacy;

namespace Hospital.Shared.Services;

public interface IAdminService
{
    Task<IReadOnlyList<Item>> GetItemsAsync(CancellationToken cancellationToken = default);
}
