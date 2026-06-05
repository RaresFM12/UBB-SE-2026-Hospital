using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;
using Microsoft.UI.Xaml.Controls;
using PatientModel = Hospital.Data.Models.Patient;

namespace Hospital.Desktop.ViewModels.ER;

public partial class RoomAssignmentViewModel : ObservableObject
{
    private readonly IERVisitService erVisitService;
    private readonly IERRoomService erRoomService;
    private readonly ITriageService triageService;
    private readonly IPatientService patientService;

    public Microsoft.UI.Xaml.XamlRoot? XamlRoot { get; set; }

    [ObservableProperty] private ObservableCollection<RoomAssignmentVisitDisplay> waitingVisits = new ObservableCollection<RoomAssignmentVisitDisplay>();
    [ObservableProperty] private ObservableCollection<ERRoom> availableRooms = new ObservableCollection<ERRoom>();
    [ObservableProperty] private RoomAssignmentVisitDisplay? selectedVisit;
    [ObservableProperty] private ERRoom? selectedRoom;
    [ObservableProperty] private PatientModel? selectedPatient;
    [ObservableProperty] private Triage? selectedTriage;
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private string actionFeedback = "Select a waiting visit and a room, then choose auto or manual assignment.";

    public RoomAssignmentViewModel(
        IERVisitService erVisitService,
        IERRoomService erRoomService,
        ITriageService triageService,
        IPatientService patientService)
    {
        this.erVisitService = erVisitService;
        this.erRoomService = erRoomService;
        this.triageService = triageService;
        this.patientService = patientService;
    }

    partial void OnSelectedVisitChanged(RoomAssignmentVisitDisplay? value)
        => _ = HandleSelectedVisitChangedAsync(value);

    private async Task HandleSelectedVisitChangedAsync(RoomAssignmentVisitDisplay? value)
    {
        if (value == null)
        {
            SelectedPatient = null;
            SelectedTriage = null;
            return;
        }
        try
        {
            SelectedPatient = value.Visit.Patient;
            SelectedTriage = value.Triage;
        }
        catch
        {
            SelectedPatient = null;
            SelectedTriage = null;
        }
    }

    [RelayCommand]
    public async Task LoadData()
    {
        try
        {
            StatusMessage = string.Empty;
            var waiting = await erVisitService.GetByStatusAsync(ERVisit.VisitStatus.WAITING_FOR_ROOM);

            var visitsWithTriage = new List<(ERVisit Visit, Triage Triage)>();
            foreach (var visit in waiting)
            {
                Triage? triage = await triageService.GetByVisitIdAsync(visit.VisitId);
                if (triage is null)
                {
                    continue;
                }

                visitsWithTriage.Add((visit, triage));
            }

            WaitingVisits = new ObservableCollection<RoomAssignmentVisitDisplay>(
                visitsWithTriage
                    .OrderByDescending(item => item.Triage.TriageLevel)
                    .ThenBy(item => item.Visit.ArrivalDateTime)
                    .Select(item => new RoomAssignmentVisitDisplay(item.Visit, item.Triage)));

            AvailableRooms = new ObservableCollection<ERRoom>(await erRoomService.GetByStatusAsync(ERRoom.RoomStatus.Available));
            StatusMessage = WaitingVisits.Count == 0
                ? "No visits are currently waiting for a room."
                : $"{WaitingVisits.Count} visit(s) are ready for room assignment.";
            ActionFeedback = WaitingVisits.Count == 0
                ? "Nothing to assign right now."
                : "Higher triage levels are shown first.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading data: {ex.Message}";
            ActionFeedback = "Room assignment data could not be loaded.";
        }
    }

    [RelayCommand]
    private async Task AssignRoom()
    {
        if (WaitingVisits.Count == 0)
        {
            await ShowDialog("No Waiting Visits", "There are no visits currently waiting for a room.");
            return;
        }
        try
        {
            bool assigned = await erVisitService.AutoAssignHighestPriorityRoomAsync();
            if (assigned)
            {
                ActionFeedback = "Auto assign succeeded. The highest-priority visit was assigned first.";
                await ShowDialog("Room Assigned", "The highest-priority visit has been automatically assigned to a matching room.");
                await LoadData();
            }
            else
            {
                ActionFeedback = "Auto assign could not find a suitable available room.";
                await ShowDialog("No Suitable Room", "No proper room matching this patient's requirements is currently available.");
            }
        }
        catch (Exception ex)
        {
            ActionFeedback = $"Auto assign failed: {ex.Message}";
            await ShowDialog("Assignment Failed", ex.Message);
        }
    }

    [RelayCommand]
    private async Task ManualAssignRoom()
    {
        if (SelectedVisit == null || SelectedRoom == null)
        {
            await ShowDialog("Selection Required", "Please select both a waiting visit and an available room.");
            return;
        }
        if (!ERRoom.StatusEquals(SelectedRoom.AvailabilityStatus, ERRoom.RoomStatus.Available))
        {
            await ShowDialog("Room Not Available", $"Room {SelectedRoom.RoomId} is '{SelectedRoom.AvailabilityStatus}'.");
            return;
        }
        if (SelectedVisit.Status != ERVisit.VisitStatus.WAITING_FOR_ROOM)
        {
            await ShowDialog("Visit Not Waiting", $"Visit {SelectedVisit.VisitId} is in '{SelectedVisit.Status}'.");
            return;
        }
        try
        {
            await erVisitService.AssignRoomAsync(SelectedVisit.VisitId, SelectedRoom.RoomId);
            ActionFeedback = $"Manual assign succeeded: visit {SelectedVisit.VisitId} -> room {SelectedRoom.RoomId}.";
            await ShowDialog("Room Assigned", $"Visit {SelectedVisit.VisitId} -> Room {SelectedRoom.RoomId} ({SelectedRoom.RoomTypeName}).");
            SelectedVisit = null;
            SelectedRoom = null;
            await LoadData();
        }
        catch (Exception ex)
        {
            ActionFeedback = $"Manual assign failed: {ex.Message}";
            await ShowDialog("Assignment Failed", ex.Message);
        }
    }

    private async Task ShowDialog(string title, string message)
    {
        if (XamlRoot == null) return;
        var dialog = new ContentDialog { Title = title, Content = message, CloseButtonText = "OK", XamlRoot = XamlRoot };
        await dialog.ShowAsync();
    }
}

public class RoomAssignmentVisitDisplay
{
    public ERVisit Visit { get; }
    public Triage Triage { get; }
    public int VisitId => Visit.VisitId;
    public string PatientName { get; }
    public string ChiefComplaint => Visit.ChiefComplaint;
    public string Status => Visit.Status;
    public int TriageLevel => Triage.TriageLevel;
    public string Specialization => Triage.Specialization;
    public string ArrivalTime => Visit.ArrivalDateTime.ToString("HH:mm");

    public RoomAssignmentVisitDisplay(ERVisit visit, Triage triage)
    {
        Visit = visit;
        Triage = triage;

        string firstName = visit.Patient?.FirstName?.Trim() ?? string.Empty;
        string lastName = visit.Patient?.LastName?.Trim() ?? string.Empty;
        string fullName = $"{firstName} {lastName}".Trim();
        PatientName = string.IsNullOrWhiteSpace(fullName) ? "Unknown patient" : fullName;
    }
}
