using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.Doctor;

public sealed record HangoutRow(
    int HangoutId,
    string Title,
    string FormattedDate,
    int ParticipantCount,
    int MaxParticipants)
{
    public string ParticipantsLabel => $"{ParticipantCount} / {MaxParticipants}";
}

public partial class HangoutsViewModel : ObservableObject
{
    private readonly IHangoutService hangoutService;

    [ObservableProperty] private ObservableCollection<HangoutRow> hangouts = new();
    [ObservableProperty] private string newTitle = string.Empty;
    [ObservableProperty] private string newDescription = string.Empty;
    [ObservableProperty] private DateTimeOffset newDate = DateTimeOffset.Now;
    [ObservableProperty] private double newMaxParticipants = 10;
    [ObservableProperty] private string statusMessage = string.Empty;

    public HangoutsViewModel(IHangoutService hangoutService)
    {
        this.hangoutService = hangoutService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        StatusMessage = string.Empty;
        try
        {
            var items = await hangoutService.GetAllHangoutsAsync();
            Hangouts.Clear();
            foreach (var hangout in items)
            {
                Hangouts.Add(new HangoutRow(
                    hangout.HangoutID,
                    hangout.Title,
                    hangout.FormattedDate,
                    hangout.HangoutParticipantEntries?.Count ?? 0,
                    hangout.MaxParticipants));
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CreateAsync()
    {
        StatusMessage = string.Empty;
        try
        {
            if (string.IsNullOrWhiteSpace(NewTitle))
            {
                StatusMessage = "Title is required.";
                return;
            }

            int max = (int)Math.Round(NewMaxParticipants);
            int id = await hangoutService.CreateHangoutAsync(
                NewTitle,
                NewDescription,
                NewDate.DateTime,
                max);

            StatusMessage = $"Created hangout #{id}.";
            NewTitle = string.Empty;
            NewDescription = string.Empty;
            NewDate = DateTimeOffset.Now;
            NewMaxParticipants = 10;

            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
