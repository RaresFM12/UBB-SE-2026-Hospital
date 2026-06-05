using Hospital.Data.Models;
using Hospital.Data.Models;

namespace Hospital.Shared.Proxies;

public interface IPrescriptionApiClient
{
    Task<List<Prescription>> GetLatestPrescriptionsAsync(int n, int page, CancellationToken cancellationToken = default);
    Task<List<Prescription>> ApplyFilterAsync(PrescriptionFilter filter, CancellationToken cancellationToken = default);
    Task<Prescription?> GetPrescriptionDetailsAsync(int id, CancellationToken cancellationToken = default);
}


