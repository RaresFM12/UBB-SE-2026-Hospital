using Microsoft.Extensions.Logging;
using SharedBilling = Hospital.Shared.Services.IBillingService;

namespace Hospital.Services.PatientEr;

public class AuditingBillingServiceDecorator(
    BillingService inner,
    ILogger<AuditingBillingServiceDecorator> logger) : IBillingService, SharedBilling
{
    public async Task<decimal> ComputeBasePriceAsync(int patientId, int recordId)
    {
        logger.LogInformation("ComputeBasePrice — patient {PatientId}, record {RecordId}", patientId, recordId);
        decimal result = await inner.ComputeBasePriceAsync(patientId, recordId);
        logger.LogInformation("ComputeBasePrice result: {Price}", result);
        return result;
    }

    public async Task<decimal> ApplyDiscountAsync(decimal basePrice, int discount)
    {
        logger.LogInformation("ApplyDiscount — base {Base}, discount {Discount}%", basePrice, discount);
        decimal result = await inner.ApplyDiscountAsync(basePrice, discount);
        logger.LogInformation("ApplyDiscount result: {Final}", result);
        return result;
    }

    public async Task<decimal> PersistDiscountAsync(int recordId, decimal basePrice, int discount)
    {
        logger.LogInformation("PersistDiscount — record {RecordId}, base {Base}, discount {Discount}%", recordId, basePrice, discount);
        decimal result = await inner.PersistDiscountAsync(recordId, basePrice, discount);
        logger.LogInformation("PersistDiscount committed: {Final}", result);
        return result;
    }
}
