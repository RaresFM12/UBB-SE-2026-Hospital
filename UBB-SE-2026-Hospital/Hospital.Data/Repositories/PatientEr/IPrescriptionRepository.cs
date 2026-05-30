using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;

namespace Hospital.Data.Repositories;

public interface IPrescriptionRepository
{
    Task<Prescription?> GetByIdAsync(int prescriptionId);
    Task<List<Prescription>> GetAllAsync();
    Task<List<Prescription>> GetFilteredAsync(PrescriptionFilter filter);
    Task<List<Prescription>> GetByRecordIdAsync(int recordId);
    Task<List<Prescription>> GetPotentialDrugAddictsAsync();
    Task<List<Prescription>> GetTopNAsync(int n, int page);
    Task<List<PrescriptionItem>> GetItemsAsync(int prescriptionId);
    Task MarkPoliceNotifiedAsync(int patientId);
    Task<List<int>> GetPoliceNotifiedPatientIdsAsync(IEnumerable<int> patientIds);
    Task<Prescription> CreateAsync(Prescription prescription);
    Task<Prescription> UpdateAsync(Prescription prescription);
    Task DeleteAsync(int prescriptionId);
}
