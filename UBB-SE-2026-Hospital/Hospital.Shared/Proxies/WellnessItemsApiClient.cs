using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class WellnessItemsApiClient(HttpClient httpClient) : ApiClientBase(httpClient), IWellnessItemsService, IWellnessItemsApiClient
{
    private const string BaseUri = "api/wellness-items";

    public IReadOnlyList<Item> GetWellnessItems()
        => Task.Run(async () => await GetAsync<List<Item>>(BaseUri) ?? []).GetAwaiter().GetResult();
}
