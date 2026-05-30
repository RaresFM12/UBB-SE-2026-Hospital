using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;

namespace Hospital.Data.Repositories;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(int patientId);
    Task<List<Patient>> GetAllAsync();
    Task<List<Patient>> GetFilteredAsync(PatientFilter filter);
    Task<Patient> CreateAsync(Patient patient);
    Task<Patient> UpdateAsync(Patient patient);
    Task DeleteAsync(int patientId);
}
