using Hospital.Data.Models;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.Services;

namespace Hospital.Services;

public class PrescriptionService(IPrescriptionRepository prescriptionRepository) : IPrescriptionService
{
    public Task<List<Prescription>> GetLatestPrescriptionsAsync(int n, int page)
        => prescriptionRepository.GetTopNAsync(n, page);

    public async Task<Prescription> GetPrescriptionDetailsAsync(int id)
    {
        var filter = new PrescriptionFilter { PrescriptionId = id };
        List<Prescription> prescriptions = await prescriptionRepository.GetFilteredAsync(filter);
        return prescriptions.FirstOrDefault()
            ?? throw new ArgumentException($"Prescription with ID {id} does not exist.");
    }

    public async Task<List<Prescription>> ApplyFilterAsync(PrescriptionFilter filter)
    {
        if (filter is null)
            return await prescriptionRepository.GetTopNAsync(20, 1);

        return await prescriptionRepository.GetFilteredAsync(filter);
    }

    public Dictionary<int, int> GetItemsFromPrescription(string prescriptionId, Dictionary<int, float> userDiscounts)
        => throw new NotSupportedException("This method is only available on the desktop client.");

    public Dictionary<int, int> GetCheapestPrescriptionItems(string prescriptionName, int requiredPills)
        => throw new NotSupportedException("This method is only available on the desktop client.");
}
