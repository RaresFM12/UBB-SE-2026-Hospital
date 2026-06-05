namespace Hospital.Shared.Proxies;

public interface IBillingApiClient
{
    Task<decimal> ComputeBasePriceAsync(int patientId, int recordId, CancellationToken cancellationToken = default);
    Task<decimal> ApplyDiscountAsync(int recordId, decimal basePrice, int discount, CancellationToken cancellationToken = default);
}


