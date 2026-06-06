using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Hospital.Desktop.ViewModels.ER;

public partial class TransferLogViewModel : ObservableObject
{
    private readonly ITransferLogService transferLogService;
    private readonly IERVisitService erVisitService;
    private readonly IExaminationService examinationService;

    public XamlRoot? XamlRoot { get; set; }

    [ObservableProperty] private VisitSummary? selectedVisit;
    [ObservableProperty] private ObservableCollection<VisitSummary> eligibleVisits = [];
    [ObservableProperty] private ObservableCollection<TransferLog> transferLogs = [];
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private ERExaminationSummary? examinationSummary;
    [ObservableProperty] private Visibility examinationSummaryVisibility = Visibility.Collapsed;

    public TransferLogViewModel(
        ITransferLogService transferLogService,
        IERVisitService erVisitService,
        IExaminationService examinationService)
    {
        this.transferLogService = transferLogService;
        this.erVisitService = erVisitService;
        this.examinationService = examinationService;
    }

    partial void OnSelectedVisitChanged(VisitSummary? value)
        => _ = LoadLogs();

    [RelayCommand]
    public async Task LoadLogs()
    {
        TransferLogs.Clear();
        ExaminationSummary = null;
        ExaminationSummaryVisibility = Visibility.Collapsed;
        if (SelectedVisit is null)
        {
            return;
        }

        foreach (TransferLog log in await transferLogService.GetByVisitIdAsync(SelectedVisit.VisitId))
        {
            TransferLogs.Add(log);
        }

        if (TransferLogs.Any(log =>
                string.Equals(log.Status, "SUCCESS", StringComparison.OrdinalIgnoreCase)))
        {
            ExaminationSummary =
                await examinationService.GetSummaryByVisitIdAsync(SelectedVisit.VisitId);
            ExaminationSummaryVisibility = ExaminationSummary is null
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
    }

    [RelayCommand]
    public async Task LoadData()
    {
        int? selectedVisitId = SelectedVisit?.VisitId;
        var visits = await transferLogService.GetEligibleVisitsAsync();
        EligibleVisits = new ObservableCollection<VisitSummary>(visits.Select(visit => new VisitSummary
        {
            VisitId = visit.VisitId,
            ChiefComplaint = visit.ChiefComplaint,
            Status = visit.Status,
            PatientName = visit.PatientName,
            Transferred = visit.Transferred,
        }));
        SelectedVisit = selectedVisitId is null
            ? null
            : EligibleVisits.FirstOrDefault(visit => visit.VisitId == selectedVisitId);

        StatusMessage = EligibleVisits.Count == 0
            ? "No visits are eligible for transfer."
            : $"{EligibleVisits.Count} visit(s) are eligible for transfer or closure.";
    }

    [RelayCommand]
    public async Task SendPatientData()
    {
        if (SelectedVisit is null)
        {
            await ShowDialog("Validation Error", "Please select a visit before sending.");
            return;
        }

        try
        {
            await erVisitService.TransferVisitAsync(SelectedVisit.VisitId);
            SelectedVisit.Status = ERVisit.VisitStatus.TRANSFERRED;
            SelectedVisit.Transferred = true;
            StatusMessage = "The patient was transferred.";
            await LoadLogs();
            await ShowDialog("Transfer Successful", "The patient was transferred.");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Transfer failed: {ex.Message}";
            await ShowDialog("Transfer Failed", ex.Message);
        }
    }

    [RelayCommand]
    public async Task CloseVisit()
    {
        if (SelectedVisit is null)
        {
            await ShowDialog("Validation Error", "Please select a visit before closing.");
            return;
        }

        try
        {
            await erVisitService.CloseVisitAsync(SelectedVisit.VisitId);
            await ShowDialog("Visit Closed", $"Visit {SelectedVisit.VisitId} was closed.");
            SelectedVisit = null;
            await LoadData();
        }
        catch (Exception ex)
        {
            await ShowDialog("Close Failed", ex.Message);
        }
    }

    private async Task ShowDialog(string title, string message)
    {
        if (XamlRoot is null)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot,
        };
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
