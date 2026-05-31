using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;

namespace Hospital.Shared.Services;

public interface IPrescriptionService
{
    Task<List<Prescription>> ApplyFilterAsync(PrescriptionFilter filter);
    Task<List<Prescription>> GetLatestPrescriptionsAsync(int n, int page);
    Task<Prescription> GetPrescriptionDetailsAsync(int id);
}
