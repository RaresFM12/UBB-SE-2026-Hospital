using Hospital.Data.Models;

namespace Hospital.Web.Services;

public interface IBloodCompatibilityApiClient
{
    Task<List<Patient>> GetTopCompatibleDonorsAsync(int recipientId, CancellationToken cancellationToken);
}