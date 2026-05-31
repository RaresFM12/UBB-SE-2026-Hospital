namespace Hospital.Shared.Services;

/// <summary>
/// Staff-side prescription service (pharmacy dispensing logic).
/// Named IStaffPrescriptionService to avoid collision with the PatientEr IPrescriptionService.
/// </summary>
public interface IStaffPrescriptionService
{
    Task<Dictionary<int, int>> GetItemsFromPrescriptionAsync(string prescriptionId, Dictionary<int, float> userDiscounts, CancellationToken cancellationToken = default);

    Task<Dictionary<int, int>> GetCheapestPrescriptionItemsAsync(string prescriptionName, int requiredPills, CancellationToken cancellationToken = default);
}
