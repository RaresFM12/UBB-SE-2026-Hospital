using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class PharmacyVacationApiClient(HttpClient httpClient) : ApiClientBase(httpClient), IPharmacyVacationService, IPharmacyVacationApiClient
{
    private const string PharmacistsUri = "api/staff/pharmacists";

    public async Task<IReadOnlyList<Pharmacyst>> GetPharmacistsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<Pharmacyst>>(PharmacistsUri, cancellationToken) ?? [];

    public IReadOnlyList<Pharmacyst> GetPharmacists()
        => Task.Run(async () => await GetAsync<List<Pharmacyst>>(PharmacistsUri) ?? []).GetAwaiter().GetResult();

    // No dedicated vacation endpoint is exposed by the API yet. Registering a vacation is
    // modelled as setting the pharmacist's availability via the staff availability endpoint.
    public async Task RegisterVacationAsync(int pharmacistStaffId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/staff/{pharmacistStaffId}/availability")
        {
            Content = System.Net.Http.Json.JsonContent.Create(
                new { isAvailable = false, status = "OnLeave", startDate, endDate },
                options: JsonOptions),
        };
        using HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public void RegisterVacation(int pharmacistStaffId, DateTime startDate, DateTime endDate)
        => Task.Run(async () => await RegisterVacationAsync(pharmacistStaffId, startDate, endDate)).GetAwaiter().GetResult();
}
