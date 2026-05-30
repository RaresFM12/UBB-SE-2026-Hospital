using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface IExaminationRepository
{
    Task<Examination?> GetByIdAsync(int examinationId);
    Task<List<Examination>> GetByVisitIdAsync(int visitId);
    Task<List<Examination>> GetAllAsync();
    Task<Examination> CreateAsync(Examination examination);
    Task<Examination> UpdateAsync(Examination examination);
    Task DeleteAsync(int examinationId);
}
