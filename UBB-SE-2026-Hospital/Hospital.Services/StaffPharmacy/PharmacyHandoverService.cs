using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class PharmacyHandoverService : IPharmacyHandoverService
{
    public Task<IReadOnlyList<PharmacyHandover>> GetAllPharmacyHandoversAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
