#if false
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services.StaffPharmacy;

public class PharmacyHandoverService(IPharmacyHandoverRepository pharmacyHandoverRepository) : IPharmacyHandoverService
{
    public async Task<IReadOnlyList<PharmacyHandover>> GetAllPharmacyHandoversAsync(CancellationToken cancellationToken = default)
        => await pharmacyHandoverRepository.GetAllAsync();
}
#endif
