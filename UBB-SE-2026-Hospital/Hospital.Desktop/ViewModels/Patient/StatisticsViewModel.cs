using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.Patient;

public partial class StatisticsViewModel : ObservableObject
{
    private readonly IStatisticsService statisticsService;

    [ObservableProperty] private Dictionary<string, int> activeVsArchived = new Dictionary<string, int>();
    [ObservableProperty] private Dictionary<string, int> ageDistribution = new Dictionary<string, int>();
    [ObservableProperty] private Dictionary<string, int> patientsByBloodType = new Dictionary<string, int>();
    [ObservableProperty] private Dictionary<string, int> patientsByRh = new Dictionary<string, int>();
    [ObservableProperty] private Dictionary<string, int> genderDistribution = new Dictionary<string, int>();
    [ObservableProperty] private Dictionary<string, int> consultationDistribution = new Dictionary<string, int>();
    [ObservableProperty] private Dictionary<string, int> topDiagnoses = new Dictionary<string, int>();
    [ObservableProperty] private Dictionary<string, int> mostPrescribedMeds = new Dictionary<string, int>();
    [ObservableProperty] private string statusMessage = string.Empty;

    public StatisticsViewModel(IStatisticsService statisticsService)
    {
        this.statisticsService = statisticsService;
    }

    [RelayCommand]
    private async Task LoadStatisticsAsync()
    {
        StatusMessage = string.Empty;
        try
        {
            ActiveVsArchived = await statisticsService.GetActiveVsArchivedRatioAsync();
            AgeDistribution = await statisticsService.GetAgeDistributionAsync();
            PatientsByBloodType = await statisticsService.GetPatientsByBloodTypeAsync();
            PatientsByRh = await statisticsService.GetPatientsByRhAsync();
            GenderDistribution = await statisticsService.GetPatientGenderDistributionAsync();
            ConsultationDistribution = await statisticsService.GetConsultationDistributionAsync();
            TopDiagnoses = await statisticsService.GetTopDiagnosesAsync();
            MostPrescribedMeds = await statisticsService.GetMostPrescribedMedsAsync();
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
