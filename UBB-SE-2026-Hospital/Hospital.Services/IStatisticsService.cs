namespace Hospital.Services;

public interface IStatisticsService
{
    Task<Dictionary<string, int>> GetActiveVsArchivedRatioAsync();
    Task<Dictionary<string, int>> GetAgeDistributionAsync();
    Task<Dictionary<string, int>> GetConsultationDistributionAsync();
    Task<Dictionary<string, int>> GetMostPrescribedMedsAsync();
    Task<Dictionary<string, int>> GetPatientGenderDistributionAsync();
    Task<Dictionary<string, int>> GetPatientsByBloodTypeAsync();
    Task<Dictionary<string, int>> GetPatientsByRhAsync();
    Task<Dictionary<string, int>> GetTopDiagnosesAsync();
}
