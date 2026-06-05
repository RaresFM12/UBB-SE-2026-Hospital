using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Data.Models;
using Hospital.Shared.Services;
using Microsoft.UI.Xaml.Controls;

namespace Hospital.Desktop.ViewModels.ER;

public partial class TransferLogViewModel : ObservableObject
{
    private readonly ITransferLogService transferLogService;
    private readonly IERVisitService erVisitService;

    public Action? ClearGridSelection { get; set; }
    public Action? RefreshGrid { get; set; }
    public Microsoft.UI.Xaml.XamlRoot? XamlRoot { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedVisit))]
    private VisitSummary? selectedVisit;

    [ObservableProperty] private ObservableCollection<VisitSummary> eligibleVisits = new ObservableCollection<VisitSummary>();
    [ObservableProperty] private ObservableCollection<TransferLog> transferLogs = new ObservableCollection<TransferLog>();
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private bool canRetry = false;

    public bool HasSelectedVisit => selectedVisit != null;

    public TransferLogViewModel(
        ITransferLogService transferLogService,
        IERVisitService erVisitService)
    {
        this.transferLogService = transferLogService;
        this.erVisitService = erVisitService;
    }

    [RelayCommand]
    public async Task LoadLogs()
    {
        TransferLogs.Clear();
        CanRetry = false;
        if (SelectedVisit == null) return;

        var logs = await transferLogService.GetByVisitIdAsync(SelectedVisit.VisitId);
        foreach (var log in logs)
        {
            TransferLogs.Add(log);
        }

        var latest = TransferLogs.FirstOrDefault();
        if (latest != null && latest.Status == "FAILED")
        {
            CanRetry = true;
        }
    }

    partial void OnSelectedVisitChanged(VisitSummary? value)
    {
        _ = LoadLogs();
    }

    [RelayCommand]
    public async Task LoadData()
    {
        SelectedVisit = null;
        TransferLogs.Clear();
        StatusMessage = string.Empty;
        CanRetry = false;

        var freshList = new ObservableCollection<VisitSummary>();
        var eligible = await transferLogService.GetEligibleVisitsAsync();

        foreach (var ev in eligible)
        {
            freshList.Add(new VisitSummary
            {
                VisitId = ev.VisitId,
                ChiefComplaint = ev.ChiefComplaint,
                Status = ev.Status,
                PatientName = ev.PatientName,
                Transferred = ev.Transferred,
            });
        }

        EligibleVisits = freshList;
        StatusMessage = EligibleVisits.Count == 0
            ? "No visits are ready for transfer yet. A visit must reach IN_EXAMINATION first."
            : $"{EligibleVisits.Count} visit(s) are ready for transfer or closure.";
    }

    [RelayCommand]
    public async Task SendPatientData()
    {
        if (SelectedVisit == null)
        {
            await ShowDialog("Validation Error", "Please select a visit before sending.");
            return;
        }
        if (SelectedVisit.Status != ERVisit.VisitStatus.IN_EXAMINATION)
        {
            await ShowDialog("Validation Error", "Transfer is only allowed for visits with status IN_EXAMINATION.");
            return;
        }
        if (SelectedVisit.Transferred)
        {
            await ShowDialog("Validation Error", "This patient already has a successful transfer.");
            return;
        }

        try
        {
            await erVisitService.TransferVisitAsync(SelectedVisit.VisitId);
            SelectedVisit.Status = ERVisit.VisitStatus.TRANSFERRED;
            SelectedVisit.Transferred = true;
            StatusMessage = "SUCCESS";
            CanRetry = false;
            await ShowDialog("Transfer Successful", $"Patient data for Visit {SelectedVisit.VisitId} has been successfully sent.");
        }
        catch (Exception ex)
        {
            StatusMessage = $"FAILED - {ex.Message}";
            CanRetry = true;
            await ShowDialog("Transfer Failed", $"Transfer failed: {ex.Message}");
        }
        finally
        {
            await LoadLogs();
        }
    }

    [RelayCommand]
    public async Task RetryTransfer()
    {
        if (SelectedVisit == null) return;
        try
        {
            await erVisitService.RetryTransferAsync(SelectedVisit.VisitId);
            SelectedVisit.Status = ERVisit.VisitStatus.TRANSFERRED;
            SelectedVisit.Transferred = true;
            StatusMessage = "Retry SUCCESS";
            CanRetry = false;
            await ShowDialog("Retry Successful", $"Patient data for Visit {SelectedVisit.VisitId} was successfully sent on retry.");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Retry FAILED - {ex.Message}";
            await ShowDialog("Retry Failed", $"Retry failed: {ex.Message}");
        }
        finally
        {
            await LoadLogs();
        }
    }

    [RelayCommand]
    public async Task CloseVisit()
    {
        if (SelectedVisit == null)
        {
            await ShowDialog("Validation Error", "Please select a visit before closing.");
            return;
        }
        if (SelectedVisit.Status != ERVisit.VisitStatus.IN_EXAMINATION)
        {
            await ShowDialog("Validation Error", "Closing is only allowed for visits with status IN_EXAMINATION.");
            return;
        }

        try
        {
            await erVisitService.CloseVisitAsync(SelectedVisit.VisitId);
            SelectedVisit.Status = ERVisit.VisitStatus.CLOSED;
            await ShowDialog("Visit Closed", $"Visit {SelectedVisit.VisitId} for {SelectedVisit.PatientName} has been closed successfully.");
        }
        catch (Exception ex)
        {
            await ShowDialog("Close Failed", $"Could not close visit: {ex.Message}");
        }
    }

    private async Task ShowDialog(string title, string message)
    {
        if (XamlRoot == null) return;
        var dialog = new ContentDialog { Title = title, Content = message, CloseButtonText = "OK", XamlRoot = XamlRoot };
        await dialog.ShowAsync();
    }
}

public partial class VisitSummary : ObservableObject
{
    [ObservableProperty] private int visitId;
    [ObservableProperty] private string patientName = string.Empty;
    [ObservableProperty] private string chiefComplaint = string.Empty;
    [ObservableProperty] private string status = string.Empty;
    [ObservableProperty] private bool transferred;
}
