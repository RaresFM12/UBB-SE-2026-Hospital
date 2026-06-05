using Hospital.Data.Models;
using Hospital.Data.Models;

namespace Hospital.Shared.Services;

public interface IExaminationService
{
    Task<List<Examination>> GetAllAsync();
    Task<Examination?> GetByIdAsync(int id);
    Task<List<Examination>> GetByVisitIdAsync(int visitId);
    Task<Examination> CreateAsync(Examination examination);
    Task<Examination> UpdateAsync(Examination examination);
    Task DeleteAsync(int id);
    Task<List<ERVisit>> GetEligibleVisitsAsync();
    Task<List<Examination>> GetPatientHistoryAsync(int patientId);
    Task<ERExaminationSummary?> GetSummaryByVisitIdAsync(int visitId);
}
