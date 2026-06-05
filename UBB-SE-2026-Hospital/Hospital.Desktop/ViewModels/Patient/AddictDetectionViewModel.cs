using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.Patient;

public partial class AddictDetectionViewModel : ObservableObject
{
    private readonly IAddictDetectionService addictDetectionService;

    [ObservableProperty] private ObservableCollection<AddictCandidateRow> candidates = new ObservableCollection<AddictCandidateRow>();
    [ObservableProperty] private string statusMessage = string.Empty;

    public AddictDetectionViewModel(IAddictDetectionService addictDetectionService)
    {
        this.addictDetectionService = addictDetectionService;
    }

    [RelayCommand]
    private async Task LoadCandidatesAsync()
    {
        Candidates.Clear();
        StatusMessage = string.Empty;
        try
        {
            var list = await addictDetectionService.GetAddictCandidatesAsync();
            foreach (var c in list)
            {
                Candidates.Add(new AddictCandidateRow
                {
                    PatientId = c.PatientId,
                    FullName = $"{c.FirstName} {c.LastName}",
                    RiskScore = 0,
                });
            }
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task GenerateReportAsync(AddictCandidateRow candidate)
    {
        if (candidate == null) return;
        try
        {
            var report = await addictDetectionService.BuildPoliceReportAsync(candidate.PatientId);
            StatusMessage = $"Report generated for {candidate.FullName}: {report}";
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}

public class AddictCandidateRow
{
    public int PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int RiskScore { get; set; }
}
