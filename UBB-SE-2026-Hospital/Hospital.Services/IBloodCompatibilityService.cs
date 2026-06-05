using Hospital.Data.Models;
namespace Hospital.Services;

public interface IBloodCompatibilityService
{
    Task<List<Hospital.Data.Models.Patient>> GetTopCompatibleDonorsAsync(int recipientId);
    int CalculateScore(Patient donor, Patient recipient);
    bool IsBloodMatch(BloodType? donor, BloodType receiver);
    bool IsRhMatch(Rh? donor, Rh receiver);
}
