using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IPharmacyVacationService
{
    Task<IReadOnlyList<Pharmacyst>> GetPharmacistsAsync(CancellationToken cancellationToken = default);

    Task RegisterVacationAsync(int pharmacistStaffId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
