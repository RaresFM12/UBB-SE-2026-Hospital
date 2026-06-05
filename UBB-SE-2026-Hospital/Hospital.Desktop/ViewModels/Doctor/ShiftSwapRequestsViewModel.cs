using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.Doctor;

public sealed record ShiftSwapRow(int SwapId, int ShiftId, int ColleagueId, string Status, DateTime RequestedAt);

public partial class ShiftSwapRequestsViewModel : ObservableObject
{
    private readonly IShiftSwapService shiftSwapService;
    private readonly ICurrentUserService currentUserService;

    [ObservableProperty] private ObservableCollection<ShiftSwapRow> requests = new();
    [ObservableProperty] private string shiftIdText = string.Empty;
    [ObservableProperty] private string colleagueIdText = string.Empty;
    [ObservableProperty] private string statusMessage = string.Empty;

    public ShiftSwapRequestsViewModel(IShiftSwapService shiftSwapService, ICurrentUserService currentUserService)
    {
        this.shiftSwapService = shiftSwapService;
        this.currentUserService = currentUserService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        StatusMessage = string.Empty;
        try
        {
            int currentUserId = currentUserService.UserId;
            var all = await shiftSwapService.GetAllShiftSwapRequestsAsync();
            var rows = all
                .Where(r => (r.Requester?.StaffId ?? 0) == currentUserId)
                .Select(r => new ShiftSwapRow(
                    r.SwapId,
                    r.Shift?.Id ?? 0,
                    r.Colleague?.StaffId ?? 0,
                    r.Status.ToString(),
                    r.RequestedAt))
                .ToList();

            Requests = new ObservableCollection<ShiftSwapRow>(rows);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RequestSwapAsync()
    {
        StatusMessage = string.Empty;
        try
        {
            if (!int.TryParse(ShiftIdText, out int shiftId))
            {
                StatusMessage = "Please enter a valid Shift Id.";
                return;
            }

            if (!int.TryParse(ColleagueIdText, out int colleagueId))
            {
                StatusMessage = "Please enter a valid Colleague Id.";
                return;
            }

            int currentUserId = currentUserService.UserId;
            await shiftSwapService.CreateShiftSwapRequestAsync(
                shiftId,
                currentUserId,
                colleagueId,
                DateTime.Now,
                ShiftSwapRequestStatus.PENDING);

            ShiftIdText = string.Empty;
            ColleagueIdText = string.Empty;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
