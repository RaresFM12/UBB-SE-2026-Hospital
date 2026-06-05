using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Shared.Proxies;

namespace Hospital.Desktop.ViewModels.Admin;

public partial class GhostViewModel : ObservableObject
{
    private readonly IGhostApiClient ghostApiClient;

    [ObservableProperty] private int sightingCount;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(CanReportSighting))] private bool exorcismTriggered;
    [ObservableProperty] private string statusMessage = string.Empty;

    public bool CanReportSighting => !ExorcismTriggered;

    public GhostViewModel(IGhostApiClient ghostApiClient)
    {
        this.ghostApiClient = ghostApiClient;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        StatusMessage = string.Empty;
        try
        {
            var status = await ghostApiClient.GetExorcismStatusAsync(CancellationToken.None);
            ApplyStatus(status);
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ReportSightingAsync()
    {
        StatusMessage = string.Empty;
        try
        {
            var status = await ghostApiClient.ReportSightingAsync(CancellationToken.None);
            ApplyStatus(status);
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private void ApplyStatus(GhostStatusDto status)
    {
        SightingCount = status.sightingCount;
        ExorcismTriggered = status.exorcismTriggered;
        StatusMessage = ExorcismTriggered
            ? "TOO MANY GHOSTS SIGHTED! The hospital is under spectral lockdown. Call the priest immediately!"
            : "All clear.";
    }
}
