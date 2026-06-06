namespace Hospital.Shared.Proxies;

public interface IStatisticsApiClient
{
    Task<Dictionary<string, int>> GetActiveVsArchivedRatioAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetAgeDistributionAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetPatientGenderDistributionAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetConsultationDistributionAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetTopDiagnosesAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetMostPrescribedMedsAsync(CancellationToken cancellationToken = default);
}


