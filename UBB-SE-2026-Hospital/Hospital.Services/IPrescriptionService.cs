using Hospital.Data.Models;
using Hospital.Data.Models;

namespace Hospital.Services;

public interface IPrescriptionService
{
    Task<List<Prescription>> ApplyFilterAsync(PrescriptionFilter filter);
    Task<List<Prescription>> GetLatestPrescriptionsAsync(int n, int page);
    Task<Prescription> GetPrescriptionDetailsAsync(int id);
}
