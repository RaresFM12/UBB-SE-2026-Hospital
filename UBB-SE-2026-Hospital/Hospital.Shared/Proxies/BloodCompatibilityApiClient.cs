using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class BloodCompatibilityApiClient : ApiClientBase, IBloodCompatibilityApiClient, IBloodCompatibilityService
{
    private const string BaseUri = "api/bloodcompatibilities";

    public BloodCompatibilityApiClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<List<Patient>> GetTopCompatibleDonorsAsync(int recipientId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await PostAsync<object, List<Patient>>(
                $"{BaseUri}/top-donors",
                new { RecipientId = recipientId },
                cancellationToken) ?? new List<Patient>();
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the blood compatibility API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The blood compatibility API request timed out.");
        }
    }

    public async Task<List<Patient>> GetTopCompatibleDonorsAsync(int recipientId)
        => await PostAsync<object, List<Patient>>($"{BaseUri}/top-donors", new { RecipientId = recipientId }) ?? [];

    private const int NoCompatibilityScore = 0;
    private const int ExactBloodRhMatchScore = 50;
    private const int PartialBloodRhMatchScore = 25;
    private const int MaxAgeScore = 30;
    private const int AgeScoreStepYears = 5;
    private const int SameSexScore = 20;
    private const int DifferentSexScore = 10;

    public int CalculateScore(Patient donor, Patient recipient)
    {
        if (donor.MedicalHistory is null || recipient.MedicalHistory is null)
            return NoCompatibilityScore;

        int total = donor.MedicalHistory.BloodType == recipient.MedicalHistory.BloodType
            && donor.MedicalHistory.Rh == recipient.MedicalHistory.Rh
            ? ExactBloodRhMatchScore
            : PartialBloodRhMatchScore;

        int ageGap = Math.Abs(donor.DateOfBirth.Year - recipient.DateOfBirth.Year);
        total += Math.Max(NoCompatibilityScore, MaxAgeScore - ageGap / AgeScoreStepYears * AgeScoreStepYears);
        total += donor.Sex == recipient.Sex ? SameSexScore : DifferentSexScore;

        return total;
    }

    public bool IsBloodMatch(BloodType? donor, BloodType receiver)
    {
        if (donor is null) return false;
        return donor switch
        {
            BloodType.O => true,
            BloodType.A => receiver is BloodType.A or BloodType.AB,
            BloodType.B => receiver is BloodType.B or BloodType.AB,
            BloodType.AB => receiver == BloodType.AB,
            _ => false,
        };
    }

    public bool IsRhMatch(Rh? donor, Rh receiver)
    {
        if (donor is null) return false;
        return receiver != Rh.Negative || donor == Rh.Negative;
    }
}
