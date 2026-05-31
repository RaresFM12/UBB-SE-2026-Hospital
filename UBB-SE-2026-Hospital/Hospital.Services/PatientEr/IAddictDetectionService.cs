using Hospital.Data.Models;

namespace Hospital.Services.PatientEr;

public interface IAddictDetectionService
{
    Task<List<Patient>> GetAddictCandidatesAsync();
    Task<string> BuildPoliceReportAsync(int patientId);
    Task<string> GetChronicConditionsAsync(int patientId);
    Task MarkPoliceNotifiedAsync(int patientId);
}
