using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IBloodCompatibilityService
{
    Task<List<Patient>> GetTopCompatibleDonorsAsync(int recipientId);
}
