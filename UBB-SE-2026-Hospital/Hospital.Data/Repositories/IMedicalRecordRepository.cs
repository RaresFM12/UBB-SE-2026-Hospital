using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface IMedicalRecordRepository
{
    Task<MedicalRecord?> GetByIdAsync(int recordId);
    Task<List<MedicalRecord>> GetByMedicalHistoryIdAsync(int medicalHistoryId);
    Task<List<MedicalRecord>> GetAllAsync();
    Task<MedicalRecord> CreateAsync(MedicalRecord record);
    Task<MedicalRecord> UpdateAsync(MedicalRecord record);
    Task DeleteAsync(int recordId);
}
