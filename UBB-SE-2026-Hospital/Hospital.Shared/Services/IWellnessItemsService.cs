using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IWellnessItemsService
{
    Task<IReadOnlyList<Item>> GetWellnessItemsAsync(CancellationToken cancellationToken = default);
}
