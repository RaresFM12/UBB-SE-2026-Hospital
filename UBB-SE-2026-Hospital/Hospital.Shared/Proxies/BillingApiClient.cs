using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Shared.Proxies;

public class BillingApiClient : ApiClientBase, IBillingApiClient, IBillingService
{
    private const string BaseUri = "api/billing";

    public BillingApiClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<decimal> ComputeBasePriceAsync(int patientId, int recordId, CancellationToken cancellationToken)
    {
        try
        {
            return await GetAsync<decimal>($"{BaseUri}/base-price/{patientId}/{recordId}", cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the billing API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The billing API request timed out or was interrupted.");
        }
    }

    public async Task<decimal> ApplyDiscountAsync(int recordId, decimal basePrice, int discount, CancellationToken cancellationToken)
    {
        try
        {
            return await PostAsync<ApplyDiscountRequest, decimal>(
                $"{BaseUri}/discount/{recordId}",
                new ApplyDiscountRequest { BasePrice = basePrice, Discount = discount },
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Could not connect to the billing API.");
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("The billing API request timed out or was interrupted.");
        }
    }

    // IBillingService methods
    public async Task<decimal> ComputeBasePriceAsync(int patientId, int recordId)
        => await GetAsync<decimal>($"{BaseUri}/base-price/{patientId}/{recordId}");

    public async Task<decimal> ApplyDiscountAsync(decimal basePrice, int discount)
        => await PostAsync<ApplyDiscountRequest, decimal>($"{BaseUri}/discount", new ApplyDiscountRequest { BasePrice = basePrice, Discount = discount });

    public async Task<decimal> PersistDiscountAsync(int recordId, decimal basePrice, int discount)
        => await PostAsync<object, decimal>($"{BaseUri}/persist-discount", new { recordId, basePrice, discount });
}
