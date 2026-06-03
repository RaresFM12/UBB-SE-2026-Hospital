using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Hospital.Desktop.ViewModels.ER;

public partial class QueueViewModel : ObservableObject
{
    private readonly IERVisitService erVisitService;
    private readonly ITriageService triageService;

    [ObservableProperty]
    private ObservableCollection<QueueItemDisplay> activeVisits = new ObservableCollection<QueueItemDisplay>();

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public bool HasActiveVisits => ActiveVisits.Count > 0;

    public QueueViewModel(IERVisitService erVisitService, ITriageService triageService)
    {
        this.erVisitService = erVisitService;
        this.triageService = triageService;
    }

    [RelayCommand]
    private async Task LoadQueue()
    {
        try
        {
            var waitingVisits = await erVisitService.GetByStatusAsync(ERVisit.VisitStatus.WAITING_FOR_ROOM);

            var refreshedQueue = new ObservableCollection<QueueItemDisplay>();
            foreach (var visit in waitingVisits)
            {
                Triage? triage = await triageService.GetByVisitIdAsync(visit.VisitId);
                if (triage == null)
                {
                    continue;
                }

                refreshedQueue.Add(new QueueItemDisplay(visit, triage));
            }

            refreshedQueue = new ObservableCollection<QueueItemDisplay>(
                refreshedQueue
                    .OrderByDescending(item => item.TriageLevel)
                    .ThenBy(item => item.ArrivalTime));

            ActiveVisits = refreshedQueue;
            StatusMessage = HasActiveVisits
                ? $"{ActiveVisits.Count} visit(s) currently waiting for room assignment."
                : "No visits are currently waiting for room assignment.";
            OnPropertyChanged(nameof(HasActiveVisits));
        }
        catch (Exception ex)
        {
            ActiveVisits.Clear();
            StatusMessage = $"Queue could not be loaded: {ex.Message}";
            OnPropertyChanged(nameof(HasActiveVisits));
            await ShowDialog("Queue Load Failed", ex.Message);
        }
    }

    [RelayCommand]
    private Task RefreshQueue() => LoadQueue();

    private static async Task ShowDialog(string title, string content)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = "OK",
            XamlRoot = ((App)Application.Current).CurrentWindow?.Content?.XamlRoot,
        };
        await dialog.ShowAsync();
    }
}

public class QueueItemDisplay
{
    public int VisitId { get; }
    public string PatientName { get; }
    public string ChiefComplaint { get; }
    public int TriageLevel { get; }
    public string Specialization { get; }
    public string ArrivalTime { get; }

    public QueueItemDisplay(ERVisit visit, Triage triage)
    {
        VisitId = visit.VisitId;
        string firstName = visit.Patient?.FirstName?.Trim() ?? string.Empty;
        string lastName = visit.Patient?.LastName?.Trim() ?? string.Empty;
        string fullName = $"{firstName} {lastName}".Trim();

        PatientName = string.IsNullOrWhiteSpace(fullName) ? "Unknown patient" : fullName;
        ChiefComplaint = visit.ChiefComplaint;
        TriageLevel = triage.TriageLevel;
        Specialization = triage.Specialization;
        ArrivalTime = visit.ArrivalDateTime.ToString("HH:mm");
    }
}
