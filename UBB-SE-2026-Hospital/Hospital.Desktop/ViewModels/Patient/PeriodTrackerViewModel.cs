using System;
using System.Collections.ObjectModel;
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
    private readonly IOrderService orderService;

    // Cycle settings
    [ObservableProperty] private DateTimeOffset startPeriodDate = DateTimeOffset.Now;
    [ObservableProperty] private double cycleDays = 28;
    [ObservableProperty] private double periodLasts = 5;
    [ObservableProperty] private int premenstrualSyndromeOption;
    [ObservableProperty] private string statusMessage = string.Empty;

    // Dashboard cards
    [ObservableProperty] private string todayDate = string.Empty;
    [ObservableProperty] private string currentPhase = string.Empty;
    [ObservableProperty] private string nextPeriodDate = string.Empty;
    [ObservableProperty] private string nextPeriodDistance = string.Empty;
    [ObservableProperty] private string ovulationDate = string.Empty;
    [ObservableProperty] private string ovulationDistance = string.Empty;

    // Month/Cycle header
    [ObservableProperty] private string currentMonthName = string.Empty;

    // Cycle interval texts
    [ObservableProperty] private string periodIntervalText = string.Empty;
    [ObservableProperty] private string lowFertilityIntervalText = string.Empty;
    [ObservableProperty] private string ovulationIntervalText = string.Empty;
    [ObservableProperty] private string pmsIntervalText = string.Empty;

    // Discount
    [ObservableProperty] private bool isDiscountActive;
    [ObservableProperty] private string discountBadgeText = string.Empty;

    // Navigation offset
    [ObservableProperty] private int monthOffset;

    // Description
    [ObservableProperty] private string description = string.Empty;

    // Notes
    [ObservableProperty] private string newNote = string.Empty;
    [ObservableProperty] private ObservableCollection<PeriodTrackerNoteRow> notes = new();

    // Wellness items
    [ObservableProperty] private ObservableCollection<PeriodTrackerShopItemSnapshot> wellnessItems = new();

    public PeriodTrackerViewModel(
        IPeriodTrackerService periodTrackerService,
        ICurrentUserService currentUserService,
        IOrderService orderService)
    {
        this.periodTrackerService = periodTrackerService;
        this.currentUserService = currentUserService;
        this.orderService = orderService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        StatusMessage = string.Empty;
        try
        {
            int userId = currentUserService.UserId;
            User user = await periodTrackerService.GetUserAsync(userId);
            StartPeriodDate = new DateTimeOffset(user.StartPeriodDate.ToDateTime(TimeOnly.MinValue));
            CycleDays = user.CycleDays;
            PeriodLasts = user.PeriodLasts;
            PremenstrualSyndromeOption = user.PremenstrualSyndromeOption;

            PeriodTrackerDashboardSnapshot snapshot = periodTrackerService.GetDashboardSnapshot(userId, MonthOffset);

            // Dashboard cards
            TodayDate = DateTime.Today.ToString("dd.MM.yyyy");
            CurrentPhase = snapshot.CurrentPhaseString;
            NextPeriodDate = snapshot.NextPeriodDateString;
            NextPeriodDistance = snapshot.NextPeriodDistanceString;

            // Ovulation date: compute from start
            DateTime computedStart = snapshot.StartPeriodDate.Date;
            while (computedStart.AddDays(snapshot.CycleDays) <= DateTime.Today)
            {
                computedStart = computedStart.AddDays(snapshot.CycleDays);
            }
            while (computedStart > DateTime.Today)
            {
                computedStart = computedStart.AddDays(-snapshot.CycleDays);
            }
            computedStart = computedStart.AddDays(MonthOffset * snapshot.CycleDays);
            DateTime ovulationStart = computedStart.AddDays(11);
            OvulationDate = ovulationStart.ToString("dd.MM.yyyy");
            OvulationDistance = snapshot.OvulationDistanceString;

            // Month header
            CurrentMonthName = snapshot.CurrentMonthName;

            // Interval texts
            PeriodIntervalText = snapshot.PeriodIntervalText;
            LowFertilityIntervalText = snapshot.LowFertilityIntervalText;
            OvulationIntervalText = snapshot.OvulationIntervalText;
            PmsIntervalText = snapshot.PmsIntervalText;

            // Discount
            IsDiscountActive = snapshot.IsInMenstrualPhase;
            DiscountBadgeText = snapshot.IsInMenstrualPhase ? "20% discount active" : string.Empty;

            // Description
            Description = string.Empty;

            // Notes
            Notes = new ObservableCollection<PeriodTrackerNoteRow>();
            foreach (var note in await periodTrackerService.GetNotesAsync(userId))
            {
                Notes.Add(new PeriodTrackerNoteRow(note.Key, note.Value.Body, note.Value.IsDone));
            }

            // Wellness items
            WellnessItems = new ObservableCollection<PeriodTrackerShopItemSnapshot>();
            if (snapshot.ShopItems != null)
            {
                foreach (var shopItem in snapshot.ShopItems)
                {
                    if (shopItem.RawItem != null)
                    {
                        WellnessItems.Add(shopItem);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        StatusMessage = string.Empty;
        try
        {
            await periodTrackerService.UpdatePeriodTrackerAsync(
                currentUserService.UserId,
                StartPeriodDate,
                CycleDays,
                PeriodLasts,
                PremenstrualSyndromeOption);
            StatusMessage = "Period tracker saved.";
            MonthOffset = 0;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving tracker: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task PreviousCycleAsync()
    {
        MonthOffset--;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task NextCycleAsync()
    {
        MonthOffset++;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task AddToBasketAsync(PeriodTrackerShopItemSnapshot? item)
    {
        if (item?.RawItem is null) return;

        try
        {
            float extraDiscount = item.HasDiscountApplied ? 20.0f : 0.0f;
            await orderService.AddItemToBasketAsync(
                currentUserService.UserId,
                item.RawItem.Id,
                1,
                extraDiscount);
            StatusMessage = $"Added {item.RawItem.Name} to basket!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error adding to basket: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddNoteAsync()
    {
        if (string.IsNullOrWhiteSpace(NewNote))
        {
            return;
        }

        try
        {
            await periodTrackerService.AddNoteAsync(currentUserService.UserId, NewNote.Trim());
            NewNote = string.Empty;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error adding note: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteNoteAsync(PeriodTrackerNoteRow? note)
    {
        if (note is null)
        {
            return;
        }

        try
        {
            await periodTrackerService.DeleteNoteAsync(currentUserService.UserId, note.NoteId);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting note: {ex.Message}";
        }
    }
}

public sealed record PeriodTrackerNoteRow(int NoteId, string Body, bool IsDone);
