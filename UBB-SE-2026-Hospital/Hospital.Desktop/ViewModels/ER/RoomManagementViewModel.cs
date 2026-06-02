using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;
using Microsoft.UI.Xaml.Controls;

namespace Hospital.Desktop.ViewModels.ER;

public partial class RoomManagementViewModel : ObservableObject
{
    private readonly IERRoomService erRoomService;

    public Microsoft.UI.Xaml.XamlRoot? XamlRoot { get; set; }

    [ObservableProperty] private Patient? selectedPatient;
    [ObservableProperty] private ERVisit? selectedVisit;
    [ObservableProperty] private Triage? selectedTriage;

    [ObservableProperty] private ObservableCollection<ERRoom> availableRooms = new ObservableCollection<ERRoom>();
    [ObservableProperty] private ObservableCollection<ERRoom> occupiedRooms = new ObservableCollection<ERRoom>();
    [ObservableProperty] private ObservableCollection<ERRoom> cleaningRooms = new ObservableCollection<ERRoom>();

    [ObservableProperty] private int totalRooms;
    [ObservableProperty] private int availableCount;
    [ObservableProperty] private int occupiedCount;
    [ObservableProperty] private int cleaningCount;

    [ObservableProperty] private ERRoom? selectedOccupiedRoom;
    [ObservableProperty] private ERRoom? selectedCleaningRoom;
    [ObservableProperty] private string statusMessage = string.Empty;

    public RoomManagementViewModel(IERRoomService erRoomService)
    {
        this.erRoomService = erRoomService;
    }

    partial void OnSelectedOccupiedRoomChanged(ERRoom? value)
        => _ = HandleSelectedRoomChangedAsync(value);

    partial void OnSelectedCleaningRoomChanged(ERRoom? value)
        => _ = HandleSelectedRoomChangedAsync(value);

    private async Task HandleSelectedRoomChangedAsync(ERRoom? value)
    {
        if (value != null)
        {
            await LoadRoomVisit(value);
        }
        else if (SelectedCleaningRoom == null && SelectedOccupiedRoom == null)
        {
            ClearVisitDetails();
        }
    }

    private async Task LoadRoomVisit(ERRoom room)
    {
        try
        {
            var details = await erRoomService.GetVisitDetailsAsync(room.RoomId);
            if (details == null)
            {
                ClearVisitDetails();
                return;
            }
            SelectedVisit = details.Visit;
            SelectedPatient = details.Patient;
            SelectedTriage = details.Triage;
        }
        catch
        {
            ClearVisitDetails();
        }
    }

    private void ClearVisitDetails()
    {
        SelectedPatient = null;
        SelectedVisit = null;
        SelectedTriage = null;
    }

    [RelayCommand]
    public async Task LoadRooms()
    {
        try
        {
            StatusMessage = string.Empty;
            AvailableRooms = new ObservableCollection<ERRoom>(await erRoomService.GetByStatusAsync(ERRoom.RoomStatus.Available));
            OccupiedRooms = new ObservableCollection<ERRoom>(await erRoomService.GetByStatusAsync(ERRoom.RoomStatus.Occupied));
            CleaningRooms = new ObservableCollection<ERRoom>(await erRoomService.GetByStatusAsync(ERRoom.RoomStatus.Cleaning));

            AvailableCount = AvailableRooms.Count;
            OccupiedCount = OccupiedRooms.Count;
            CleaningCount = CleaningRooms.Count;
            TotalRooms = AvailableCount + OccupiedCount + CleaningCount;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading rooms: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task MarkRoomAsCleaning()
    {
        if (SelectedOccupiedRoom == null)
        {
            await ShowDialog("No Room Selected", "Please select an occupied room first.");
            return;
        }
        try
        {
            await erRoomService.MarkRoomAsCleaningAsync(SelectedOccupiedRoom.RoomId);
            await ShowDialog("Room Cleaning", $"Room {SelectedOccupiedRoom.RoomId} ({SelectedOccupiedRoom.RoomTypeName}) is now being cleaned.");
            SelectedOccupiedRoom = null;
            await LoadRooms();
        }
        catch (Exception ex)
        {
            await ShowDialog("Error", ex.Message);
        }
    }

    [RelayCommand]
    private async Task MarkRoomAsCleaned()
    {
        if (SelectedCleaningRoom == null)
        {
            await ShowDialog("No Room Selected", "Please select a room in the Cleaning tab first.");
            return;
        }
        try
        {
            await erRoomService.MarkRoomAsAvailableAsync(SelectedCleaningRoom.RoomId);
            await ShowDialog("Room Ready", $"Room {SelectedCleaningRoom.RoomId} ({SelectedCleaningRoom.RoomTypeName}) is now available.");
            SelectedCleaningRoom = null;
            await LoadRooms();
        }
        catch (Exception ex)
        {
            await ShowDialog("Error", ex.Message);
        }
    }

    private async Task ShowDialog(string title, string message)
    {
        if (XamlRoot == null) return;
        var dialog = new ContentDialog { Title = title, Content = message, CloseButtonText = "OK", XamlRoot = XamlRoot };
        await dialog.ShowAsync();
    }
}
