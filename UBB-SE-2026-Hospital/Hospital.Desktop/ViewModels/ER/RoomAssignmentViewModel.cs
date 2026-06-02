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

namespace Hospital.Desktop.ViewModels.ER;

public partial class RoomAssignmentViewModel : ObservableObject
{
    private readonly IERVisitService erVisitService;
    private readonly IERRoomService erRoomService;
    private readonly ITriageService triageService;
    private readonly IPatientService patientService;

    public Microsoft.UI.Xaml.XamlRoot? XamlRoot { get; set; }

    [ObservableProperty] private ObservableCollection<ERVisit> waitingVisits = new ObservableCollection<ERVisit>();
    [ObservableProperty] private ObservableCollection<ERRoom> availableRooms = new ObservableCollection<ERRoom>();
    [ObservableProperty] private ERVisit? selectedVisit;
    [ObservableProperty] private ERRoom? selectedRoom;
    [ObservableProperty] private Patient? selectedPatient;
    [ObservableProperty] private Triage? selectedTriage;
    [ObservableProperty] private string statusMessage = string.Empty;

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

    partial void OnSelectedVisitChanged(ERVisit? value)
        => _ = HandleSelectedVisitChangedAsync(value);

    private async Task HandleSelectedVisitChangedAsync(ERVisit? value)
    {
        if (value == null)
        {
            SelectedPatient = null;
            SelectedTriage = null;
            return;
        }
        try
        {
            SelectedPatient = value.Patient;
            SelectedTriage = await triageService.GetByVisitIdAsync(value.VisitId);
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
            var triages = await triageService.GetAllAsync();
            var ordered = waiting
                .Join(triages, v => v.VisitId, t => t.Visit.VisitId, (v, t) => (v, t))
                .OrderBy(q => q.t.TriageLevel)
                .ThenBy(q => q.v.ArrivalDateTime)
                .ToList();

            WaitingVisits = new ObservableCollection<ERVisit>();
            foreach (var (visit, _) in ordered)
            {
                WaitingVisits.Add(visit);
            }

            AvailableRooms = new ObservableCollection<ERRoom>(await erRoomService.GetByStatusAsync(ERRoom.RoomStatus.Available));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading data: {ex.Message}";
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
                await ShowDialog("Room Assigned", "The highest-priority visit has been automatically assigned to a matching room.");
                await LoadData();
            }
            else
            {
                await ShowDialog("No Suitable Room", "No proper room matching this patient's requirements is currently available.");
            }
        }
        catch (Exception ex)
        {
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
            await ShowDialog("Room Assigned", $"Visit {SelectedVisit.VisitId} -> Room {SelectedRoom.RoomId} ({SelectedRoom.RoomTypeName}).");
            SelectedVisit = null;
            SelectedRoom = null;
            await LoadData();
        }
        catch (Exception ex)
        {
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
