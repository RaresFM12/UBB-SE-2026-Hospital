using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface IEvaluationsRepository
{
    Task<MedicalEvaluation?> GetByIdAsync(int evaluationId);
    Task<List<MedicalEvaluation>> GetAllAsync();
    Task<List<MedicalEvaluation>> GetByDoctorIdAsync(int doctorId);
    Task<MedicalEvaluation> CreateAsync(MedicalEvaluation evaluation);
    Task<MedicalEvaluation> UpdateAsync(MedicalEvaluation evaluation);
    Task DeleteAsync(int evaluationId);
}
