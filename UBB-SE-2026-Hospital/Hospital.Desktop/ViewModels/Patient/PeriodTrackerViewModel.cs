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

    [ObservableProperty] private DateTimeOffset startPeriodDate = DateTimeOffset.Now;
    [ObservableProperty] private double cycleDays = 28;
    [ObservableProperty] private double periodLasts = 5;
    [ObservableProperty] private int premenstrualSyndromeOption;
    [ObservableProperty] private string currentPhase = string.Empty;
    [ObservableProperty] private string nextPeriod = string.Empty;
    [ObservableProperty] private string ovulation = string.Empty;
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private string newNote = string.Empty;
    [ObservableProperty] private ObservableCollection<PeriodTrackerNoteRow> notes = new();

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
            User user = await periodTrackerService.GetUserAsync(userId);
            StartPeriodDate = new DateTimeOffset(user.StartPeriodDate.ToDateTime(TimeOnly.MinValue));
            CycleDays = user.CycleDays;
            PeriodLasts = user.PeriodLasts;
            PremenstrualSyndromeOption = user.PremenstrualSyndromeOption;

            PeriodTrackerDashboardSnapshot snapshot = periodTrackerService.GetDashboardSnapshot(userId, 0);
            CurrentPhase = snapshot.CurrentPhaseString;
            NextPeriod = snapshot.NextPeriodDateString;
            Ovulation = snapshot.OvulationDistanceString;

            Notes = new ObservableCollection<PeriodTrackerNoteRow>();
            foreach (var note in await periodTrackerService.GetNotesAsync(userId))
            {
                Notes.Add(new PeriodTrackerNoteRow(note.Key, note.Value.Body, note.Value.IsDone));
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
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving tracker: {ex.Message}";
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
