using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.ER;

public partial class QueueViewModel : ObservableObject
{
    private readonly IERVisitService erVisitService;
    private readonly ITriageService triageService;

    [ObservableProperty]
    private ObservableCollection<QueueItemDisplay> activeVisits = new ObservableCollection<QueueItemDisplay>();

    public QueueViewModel(IERVisitService erVisitService, ITriageService triageService)
    {
        this.erVisitService = erVisitService;
        this.triageService = triageService;
    }

    [RelayCommand]
    private async Task LoadQueue()
    {
        var waitingVisits = await erVisitService.GetByStatusAsync(ERVisit.VisitStatus.WAITING_FOR_ROOM);
        var triages = await triageService.GetAllAsync();
        var queue = waitingVisits
            .Join(
                triages,
                visit => visit.VisitId,
                triage => triage.Visit.VisitId,
                (visit, triage) => (visit, triage))
            .OrderBy(queueEntry => queueEntry.triage.TriageLevel)
            .ThenBy(queueEntry => queueEntry.visit.ArrivalDateTime);

        var refreshedQueue = new ObservableCollection<QueueItemDisplay>();
        foreach (var (visit, triage) in queue)
        {
            refreshedQueue.Add(new QueueItemDisplay(visit, triage));
        }

        ActiveVisits = refreshedQueue;
    }

    [RelayCommand]
    private Task RefreshQueue() => LoadQueue();
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
        PatientName = visit.Patient != null ? $"{visit.Patient.FirstName} {visit.Patient.LastName}" : "Unknown";
        ChiefComplaint = visit.ChiefComplaint;
        TriageLevel = triage.TriageLevel;
        Specialization = triage.Specialization;
        ArrivalTime = visit.ArrivalDateTime.ToString("HH:mm");
    }
}
