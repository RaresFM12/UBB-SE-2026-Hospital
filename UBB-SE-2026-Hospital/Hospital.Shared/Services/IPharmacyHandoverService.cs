using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IPharmacyHandoverService
{
    Task<IReadOnlyList<PharmacyHandover>> GetAllPharmacyHandoversAsync(CancellationToken cancellationToken = default);
}
