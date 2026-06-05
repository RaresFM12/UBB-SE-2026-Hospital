using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Proxy;

public class HttpBloodCompatibilityProxy(HttpClient httpClient) : ProxyBase(httpClient), IBloodCompatibilityService
{
    private const string BaseUri = "api/bloodcompatibilities";

    public async Task<List<Patient>> GetTopCompatibleDonorsAsync(int recipientId)
        => await PostAsync<object, List<Patient>>($"{BaseUri}/top-donors", new { RecipientId = recipientId }) ?? [];
}
