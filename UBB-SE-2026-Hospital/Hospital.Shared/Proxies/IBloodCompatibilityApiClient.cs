using Hospital.Data.Models;

namespace Hospital.Shared.Proxies;

public interface IBloodCompatibilityApiClient
{
    Task<List<Patient>> GetTopCompatibleDonorsAsync(int recipientId, CancellationToken cancellationToken);
}

