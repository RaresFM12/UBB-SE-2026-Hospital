using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class AllergyApiClient : ApiClientBase, IAllergyApiClient, IAllergyService
{
    private const string BaseUri = "api/allergies";

    public AllergyApiClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<List<Allergy>> GetAllergiesAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<Allergy>>(BaseUri, cancellationToken) ?? new List<Allergy>();

    public Task<List<Allergy>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetAllergiesAsync(cancellationToken);

    public async Task<List<Allergy>> GetAllAsync() =>
        await GetAllergiesAsync(CancellationToken.None);

    public Task<List<Allergy>> GetAllergiesAsync() =>
        GetAllergiesAsync(CancellationToken.None);
}
