using Hospital.Shared.Services;
using Hospital.Data.Models;
using Prescription = Hospital.Data.Models.Prescription;

namespace Hospital.Desktop.Proxy;

public class HttpPrescriptionProxy(HttpClient httpClient) : ProxyBase(httpClient), IPrescriptionService
{
    private const string BaseUri = "api/prescriptions";

    public Dictionary<int, int> GetItemsFromPrescription(string prescriptionId, Dictionary<int, float> userDiscounts)
        => Task.Run(async () => await PostAsync<object, Dictionary<int, int>>($"{BaseUri}/{prescriptionId}/items", new { userDiscounts }) ?? []).GetAwaiter().GetResult();

    public Dictionary<int, int> GetCheapestPrescriptionItems(string prescriptionName, int requiredPills)
        => Task.Run(async () => await GetAsync<Dictionary<int, int>>($"{BaseUri}/cheapest?name={Uri.EscapeDataString(prescriptionName)}&requiredPills={requiredPills}") ?? []).GetAwaiter().GetResult();

    public async Task<List<Prescription>> GetLatestPrescriptionsAsync(int count = 50, int page = 1)
        => await GetAsync<List<Prescription>>($"{BaseUri}/latest?n={count}&page={page}") ?? [];

    public async Task<List<Prescription>> ApplyFilterAsync(PrescriptionFilter filter)
        => await PostAsync<PrescriptionFilter, List<Prescription>>(BaseUri, filter) ?? [];

    public async Task<Prescription?> GetPrescriptionDetailsAsync(int prescriptionId)
        => await GetAsync<Prescription>($"{BaseUri}/{prescriptionId}");
}
