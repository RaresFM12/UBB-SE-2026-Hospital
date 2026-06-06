using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.Patient;

public partial class PeriodTrackerViewModel : ObservableObject
{
    private readonly IPeriodTrackerService periodTrackerService;
    private readonly ICurrentUserService currentUserService;

    private int monthOffset;

    [ObservableProperty] private bool hasPeriodTracker;
    [ObservableProperty] private string currentPhaseString = string.Empty;
    [ObservableProperty] private string nextPeriodDateString = string.Empty;
    [ObservableProperty] private string nextPeriodDistanceString = string.Empty;
    [ObservableProperty] private int currentDayOfCycle;
    [ObservableProperty] private int daysUntilOvulation;
    [ObservableProperty] private string ovulationDistanceString = string.Empty;
    [ObservableProperty] private string currentMonthName = string.Empty;
    [ObservableProperty] private string periodIntervalText = string.Empty;
    [ObservableProperty] private string lowFertilityIntervalText = string.Empty;
    [ObservableProperty] private string ovulationIntervalText = string.Empty;
    [ObservableProperty] private string pmsIntervalText = string.Empty;
    [ObservableProperty] private bool isInMenstrualPhase;
    [ObservableProperty] private string statusMessage = string.Empty;

    // Cycle configuration fields (editable)
    [ObservableProperty] private DateTimeOffset startDate = DateTimeOffset.Now;
    [ObservableProperty] private int cycleDays = 28;
    [ObservableProperty] private int periodLasts = 5;
    [ObservableProperty] private int pmsOption;

    // Note input
    [ObservableProperty] private string newNoteText = string.Empty;
    [ObservableProperty] private bool canAddNote;

    public ObservableCollection<NoteItem> Notes { get; } = new();
    public ObservableCollection<ShopItem> ShopItems { get; } = new();

    public PeriodTrackerViewModel(IPeriodTrackerService periodTrackerService, ICurrentUserService currentUserService)
    {
        this.periodTrackerService = periodTrackerService;
        this.currentUserService = currentUserService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        StatusMessage = string.Empty;
        try
        {
            int userId = currentUserService.UserId;
            var snapshot = await Task.Run(() => periodTrackerService.GetDashboardSnapshot(userId, monthOffset));

            HasPeriodTracker = snapshot.HasPeriodTracker;
            CurrentPhaseString = snapshot.CurrentPhaseString;
            NextPeriodDateString = snapshot.NextPeriodDateString;
            NextPeriodDistanceString = snapshot.NextPeriodDistanceString;
            CurrentDayOfCycle = snapshot.CurrentDayOfCycle;
            DaysUntilOvulation = snapshot.DaysUntilOvulation;
            OvulationDistanceString = snapshot.OvulationDistanceString;
            CurrentMonthName = snapshot.CurrentMonthName;
            PeriodIntervalText = snapshot.PeriodIntervalText;
            LowFertilityIntervalText = snapshot.LowFertilityIntervalText;
            OvulationIntervalText = snapshot.OvulationIntervalText;
            PmsIntervalText = snapshot.PmsIntervalText;
            IsInMenstrualPhase = snapshot.IsInMenstrualPhase;

            if (snapshot.HasPeriodTracker)
            {
                StartDate = new DateTimeOffset(snapshot.StartPeriodDate);
                CycleDays = snapshot.CycleDays;
                PeriodLasts = snapshot.PeriodLasts;
                PmsOption = snapshot.PMSOption;
            }

            Notes.Clear();
            foreach (var note in snapshot.Notes)
            {
                Notes.Add(new NoteItem { NoteId = note.NoteId, Body = note.NoteBody, IsDone = note.IsDone });
            }

            CanAddNote = Notes.Count < 4;

            ShopItems.Clear();
            foreach (var item in snapshot.ShopItems)
            {
                if (item.RawItem != null)
                {
                    ShopItems.Add(new ShopItem
                    {
                        Name = item.RawItem.Name,
                        DisplayPrice = item.DisplayPrice,
                        HasDiscount = item.HasDiscountApplied,
                        OriginalPrice = item.RawItem.Price,
                    });
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading tracker: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveCycleAsync()
    {
        StatusMessage = string.Empty;
        try
        {
            if (CycleDays < 20 || CycleDays > 45)
            {
                StatusMessage = "Cycle days must be between 20 and 45.";
                return;
            }

            if (PeriodLasts < 1 || PeriodLasts > 9)
            {
                StatusMessage = "Period length must be between 1 and 9.";
                return;
            }

            if (PmsOption < 0 || PmsOption > 3)
            {
                StatusMessage = "PMS option must be between 0 and 3.";
                return;
            }

            int userId = currentUserService.UserId;
            await periodTrackerService.UpdatePeriodTrackerAsync(userId, StartDate, CycleDays, PeriodLasts, PmsOption);
            StatusMessage = "Cycle settings saved.";
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddNoteAsync()
    {
        StatusMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(NewNoteText))
        {
            StatusMessage = "Note text cannot be empty.";
            return;
        }

        if (Notes.Count >= 4)
        {
            StatusMessage = "Maximum 4 notes allowed.";
            return;
        }

        try
        {
            int userId = currentUserService.UserId;
            await periodTrackerService.AddNoteAsync(userId, NewNoteText.Trim());
            NewNoteText = string.Empty;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error adding note: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteNoteAsync(int noteId)
    {
        try
        {
            int userId = currentUserService.UserId;
            await periodTrackerService.DeleteNoteAsync(userId, noteId);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting note: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ToggleNoteAsync(int noteId)
    {
        var note = Notes.FirstOrDefault(n => n.NoteId == noteId);
        if (note == null)
        {
            return;
        }

        try
        {
            int userId = currentUserService.UserId;
            await periodTrackerService.UpdateNoteAsync(userId, noteId, note.Body, !note.IsDone);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error updating note: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task PrevMonthAsync()
    {
        monthOffset--;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task NextMonthAsync()
    {
        monthOffset++;
        await LoadAsync();
    }

    public class NoteItem
    {
        public int NoteId { get; set; }
        public string Body { get; set; } = string.Empty;
        public bool IsDone { get; set; }
    }

    public class ShopItem
    {
        public string Name { get; set; } = string.Empty;
        public float DisplayPrice { get; set; }
        public float OriginalPrice { get; set; }
        public bool HasDiscount { get; set; }
        public string PriceDisplay => HasDiscount
            ? $"{DisplayPrice:F2} lei (was {OriginalPrice:F2})"
            : $"{DisplayPrice:F2} lei";
    }
}
