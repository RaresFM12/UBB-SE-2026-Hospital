using Hospital.Data.Models;

namespace Hospital.Shared.Proxies;

public interface IPharmacyVacationApiClient
{
    Task<IReadOnlyList<Pharmacyst>> GetPharmacistsAsync(CancellationToken cancellationToken = default);
    IReadOnlyList<Pharmacyst> GetPharmacists();
    Task RegisterVacationAsync(int pharmacistStaffId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    void RegisterVacation(int pharmacistStaffId, DateTime startDate, DateTime endDate);
}
