using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface IMedicalHistoryRepository
{
    Task<MedicalHistory?> GetByIdAsync(int medicalHistoryId);
    Task<MedicalHistory?> GetByPatientIdAsync(int patientId);
    Task<List<MedicalHistory>> GetAllAsync();
    Task<MedicalHistory> CreateAsync(MedicalHistory medicalHistory);
    Task<MedicalHistory> UpdateAsync(MedicalHistory medicalHistory);
    Task DeleteAsync(int medicalHistoryId);
}
