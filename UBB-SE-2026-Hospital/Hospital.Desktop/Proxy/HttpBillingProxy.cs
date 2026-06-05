using Hospital.Data.Models.DTOs;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Proxy;

public class HttpBillingProxy(HttpClient httpClient) : ProxyBase(httpClient), IBillingService
{
    private const string BaseUri = "api/billing";

    public async Task<decimal> ComputeBasePriceAsync(int patientId, int recordId)
        => await GetAsync<decimal>($"{BaseUri}/base-price/{patientId}/{recordId}");

    public async Task<decimal> ApplyDiscountAsync(decimal basePrice, int discount)
        => await PostAsync<ApplyDiscountRequest, decimal>($"{BaseUri}/discount", new ApplyDiscountRequest { BasePrice = basePrice, Discount = discount });

    public async Task<decimal> PersistDiscountAsync(int recordId, decimal basePrice, int discount)
        => await PostAsync<object, decimal>($"{BaseUri}/persist-discount", new { recordId, basePrice, discount });
}
