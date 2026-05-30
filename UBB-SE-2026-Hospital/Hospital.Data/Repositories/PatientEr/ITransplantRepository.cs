using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface ITransplantRepository
{
    Task<Transplant?> GetByIdAsync(int transplantId);
    Task<List<Transplant>> GetAllAsync();
    Task<List<Transplant>> GetByPatientIdAsync(int patientId);
    Task<List<TransplantMatch>> GetMatchesForTransplantAsync(int transplantId);
    Task<Transplant> CreateAsync(Transplant transplant);
    Task<Transplant> UpdateAsync(Transplant transplant);
    Task DeleteAsync(int transplantId);
    Task<TransplantMatch> CreateMatchAsync(TransplantMatch match);
    Task<TransplantMatch> UpdateMatchAsync(TransplantMatch match);
}
