using Hospital.Data.Models;

namespace Hospital.Shared.Proxies;

public interface IAddictDetectionApiClient
{
    Task<List<Patient>> GetCandidatesAsync(CancellationToken cancellationToken);
    Task<string> BuildPoliceReportAsync(int patientId, CancellationToken cancellationToken);
    Task MarkPoliceNotifiedAsync(int patientId, CancellationToken cancellationToken);
}


