using Hospital.Data.Models;
using Hospital.Data.Models;

namespace Hospital.Shared.Proxies;

public interface IPrescriptionApiClient
{
    Task<List<Prescription>> GetLatestPrescriptionsAsync(int n, int page, CancellationToken cancellationToken);
    Task<List<Prescription>> ApplyFilterAsync(PrescriptionFilter filter, CancellationToken cancellationToken);
    Task<Prescription?> GetPrescriptionDetailsAsync(int id, CancellationToken cancellationToken);
}


