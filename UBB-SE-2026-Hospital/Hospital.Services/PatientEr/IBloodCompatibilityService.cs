using Hospital.Data.Models;
namespace Hospital.Services.PatientEr;

public interface IBloodCompatibilityService
{
    Task<List<Hospital.Shared.Models.PatientEr.Patient>> GetTopCompatibleDonorsAsync(int recipientId);
    int CalculateScore(Patient donor, Patient recipient);
    bool IsBloodMatch(BloodType? donor, BloodType receiver);
    bool IsRhMatch(Rh? donor, Rh receiver);
}
