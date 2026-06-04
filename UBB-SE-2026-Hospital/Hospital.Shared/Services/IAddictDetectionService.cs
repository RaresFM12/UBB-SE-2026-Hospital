using Hospital.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hospital.Shared.Services;

public interface IAddictDetectionService
{
    Task<List<Patient>> GetAddictCandidatesAsync();
    Task<string> BuildPoliceReportAsync(int patientId);
    Task<string> GetChronicConditionsAsync(int patientId);
}
