using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.ViewModels.Doctor;

public sealed record IncomingSwapRow(int SwapId, int ShiftId, int RequesterId, DateTime RequestedAt);

public partial class IncomingSwapsViewModel : ObservableObject
{
    private readonly IShiftSwapService shiftSwapService;
    private readonly ICurrentUserService currentUserService;

    [ObservableProperty] private ObservableCollection<IncomingSwapRow> requests = new();
    [ObservableProperty] private string statusMessage = string.Empty;

    public IncomingSwapsViewModel(IShiftSwapService shiftSwapService, ICurrentUserService currentUserService)
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
                .Where(r => (r.Colleague?.StaffId ?? 0) == currentUserId
                    && r.Status == ShiftSwapRequestStatus.PENDING)
                .Select(r => new IncomingSwapRow(
                    r.SwapId,
                    r.Shift?.Id ?? 0,
                    r.Requester?.StaffId ?? 0,
                    r.RequestedAt))
                .ToList();

            Requests = new ObservableCollection<IncomingSwapRow>(rows);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AcceptAsync(IncomingSwapRow? row)
    {
        if (row is null)
        {
            return;
        }

        StatusMessage = string.Empty;
        try
        {
            await shiftSwapService.AcceptSwapRequestAsync(row.SwapId, currentUserService.UserId);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RejectAsync(IncomingSwapRow? row)
    {
        if (row is null)
        {
            return;
        }

        StatusMessage = string.Empty;
        try
        {
            await shiftSwapService.RejectSwapRequestAsync(row.SwapId, currentUserService.UserId);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
