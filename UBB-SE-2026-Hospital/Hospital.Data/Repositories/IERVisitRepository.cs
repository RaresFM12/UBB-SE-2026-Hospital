using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface IERVisitRepository
{
    Task<ERVisit?> GetByIdAsync(int visitId);
    Task<List<ERVisit>> GetAllAsync();
    Task<List<ERVisit>> GetByPatientIdAsync(int patientId);
    Task<List<ERVisit>> GetActiveVisitsAsync();
    Task<ERVisit> CreateAsync(ERVisit visit);
    Task<ERVisit> UpdateAsync(ERVisit visit);
    Task DeleteAsync(int visitId);
}
