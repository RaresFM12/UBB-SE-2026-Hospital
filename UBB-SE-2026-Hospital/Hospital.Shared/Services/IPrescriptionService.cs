using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;

namespace Hospital.Shared.Services;

public interface IPrescriptionService
{
    Dictionary<int, int> GetItemsFromPrescription(string prescriptionId, Dictionary<int, float> userDiscounts);
    Dictionary<int, int> GetCheapestPrescriptionItems(string prescriptionName, int requiredPills);
    Task<List<Prescription>> GetLatestPrescriptionsAsync(int count = 50, int page = 1);
    Task<List<Prescription>> ApplyFilterAsync(PrescriptionFilter filter);
    Task<Prescription?> GetPrescriptionDetailsAsync(int prescriptionId);
}
