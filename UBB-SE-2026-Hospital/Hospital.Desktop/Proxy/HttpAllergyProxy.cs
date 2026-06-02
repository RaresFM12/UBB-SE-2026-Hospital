using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Proxy;

public class HttpAllergyProxy(HttpClient httpClient) : ProxyBase(httpClient), IAllergyService
{
    private const string BaseUri = "api/allergies";

    public async Task<List<Allergy>> GetAllAsync()
        => await GetAsync<List<Allergy>>(BaseUri) ?? [];
}
