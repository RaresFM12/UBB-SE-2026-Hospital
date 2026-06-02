using Hospital.Shared.Services;

namespace Hospital.Desktop.Proxy;

public class HttpPrescriptionProxy(HttpClient httpClient) : ProxyBase(httpClient), IPrescriptionService
{
    private const string BaseUri = "api/prescriptions";

    public Dictionary<int, int> GetItemsFromPrescription(string prescriptionId, Dictionary<int, float> userDiscounts)
        => Task.Run(async () => await PostAsync<object, Dictionary<int, int>>($"{BaseUri}/{prescriptionId}/items", new { userDiscounts }) ?? []).GetAwaiter().GetResult();

    public Dictionary<int, int> GetCheapestPrescriptionItems(string prescriptionName, int requiredPills)
        => Task.Run(async () => await GetAsync<Dictionary<int, int>>($"{BaseUri}/cheapest?name={Uri.EscapeDataString(prescriptionName)}&requiredPills={requiredPills}") ?? []).GetAwaiter().GetResult();
}
