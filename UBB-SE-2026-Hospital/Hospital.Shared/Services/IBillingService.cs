using Hospital.Data.Models.DTOs;
using System.Threading.Tasks;

namespace Hospital.Shared.Services;

public interface IBillingService
{
    Task<decimal> ComputeBasePriceAsync(int patientId, int recordId);
    Task<decimal> ApplyDiscountAsync(decimal basePrice, int discount);
}
