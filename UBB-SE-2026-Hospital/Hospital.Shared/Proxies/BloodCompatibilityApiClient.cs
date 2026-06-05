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
}
