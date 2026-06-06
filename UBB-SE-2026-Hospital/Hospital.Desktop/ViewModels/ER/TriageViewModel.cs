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

public partial class TriageViewModel : ObservableObject
{
    private readonly ITriageService triageService;

    public ObservableCollection<ERVisit> RegisteredVisits { get; } = new ObservableCollection<ERVisit>();

    [ObservableProperty]
    private ERVisit? selectedVisit;

    [ObservableProperty]
    private int consciousness;

    [ObservableProperty]
    private int breathing;

    [ObservableProperty]
    private int bleeding;

    [ObservableProperty]
    private int injuryType;

    [ObservableProperty]
    private int painLevel;

    [ObservableProperty]
    private int calculatedSeverity;

    [ObservableProperty]
    private string calculatedSpecialization = string.Empty;

    [ObservableProperty]
    private bool isTriaged;

    public bool HasRegisteredVisits => RegisteredVisits.Count > 0;
    public bool IsNotTriaged => !IsTriaged;
    public string SelectedVisitLabel => SelectedVisit == null
        ? "none"
        : $"#{SelectedVisit.VisitId} - {SelectedVisit.Patient?.FirstName} {SelectedVisit.Patient?.LastName}".TrimEnd();
    public string RegisteredVisitsStatusText => HasRegisteredVisits
        ? $"{RegisteredVisits.Count} visit(s) available in triage."
        : "No visits are currently available in triage.";
    public string TriageResultText => CalculatedSeverity > 0
        ? $"Calculated triage level: {CalculatedSeverity}"
        : "Calculated triage level: pending";
    public string SpecializationResultText => string.IsNullOrWhiteSpace(CalculatedSpecialization)
        ? "Suggested specialization: pending"
        : $"Suggested specialization: {CalculatedSpecialization}";

    private Triage? lastTriageResult;

    public TriageViewModel(ITriageService triageService)
    {
        this.triageService = triageService;
    }

    partial void OnSelectedVisitChanged(ERVisit? value)
        => _ = HandleSelectedVisitChangedAsync(value);

    private async Task HandleSelectedVisitChangedAsync(ERVisit? value)
    {
        if (value == null)
        {
            IsTriaged = false;
            CalculatedSeverity = 0;
            CalculatedSpecialization = string.Empty;
            OnPropertyChanged(nameof(SelectedVisitLabel));
            OnPropertyChanged(nameof(TriageResultText));
            OnPropertyChanged(nameof(SpecializationResultText));
            return;
        }

        IsTriaged = value.Status == ERVisit.VisitStatus.TRIAGED;

        if (IsTriaged)
        {
            var triage = await triageService.GetByVisitIdAsync(value.VisitId);
            if (triage != null)
            {
                CalculatedSeverity = triage.TriageLevel;
                CalculatedSpecialization = triage.Specialization;
            }
        }
        else
        {
            CalculatedSeverity = 0;
            CalculatedSpecialization = string.Empty;
        }

        OnPropertyChanged(nameof(SelectedVisitLabel));
        OnPropertyChanged(nameof(TriageResultText));
        OnPropertyChanged(nameof(SpecializationResultText));
    }

    [RelayCommand]
    private Task LoadVisitsForTriage()
        => LoadVisitsForTriageAsync();

    [RelayCommand]
    private async Task PerformTriage()
    {
        if (SelectedVisit == null)
        {
            await ShowDialog("No Visit Selected", "Please select a visit from the list before performing triage.");
            return;
        }

        if (SelectedVisit.Status == ERVisit.VisitStatus.TRIAGED)
        {
            await ShowDialog("Triage already performed", "The triage for this visit has already been performed.");
            return;
        }

        if (Consciousness == 0 || Breathing == 0 || Bleeding == 0 ||
            InjuryType == 0 || PainLevel == 0)
        {
            await ShowDialog("Incomplete Parameters", "Please select a value for all 5 triage parameters.");
            return;
        }

        try
        {
            var parameters = new PerformTriageRequest
            {
                VisitId = SelectedVisit.VisitId,
                Consciousness = Consciousness,
                Breathing = Breathing,
                Bleeding = Bleeding,
                InjuryType = InjuryType,
                PainLevel = PainLevel
            };

            lastTriageResult = await triageService.CreateTriageAsync(SelectedVisit.VisitId, parameters);

            CalculatedSeverity = lastTriageResult.TriageLevel;
            CalculatedSpecialization = lastTriageResult.Specialization;
            IsTriaged = true;
            OnPropertyChanged(nameof(TriageResultText));
            OnPropertyChanged(nameof(SpecializationResultText));

            await ShowDialog("Triage Complete",
                $"Visit {SelectedVisit.VisitId} triaged successfully.\n" +
                $"Severity Level: {CalculatedSeverity}\n" +
                $"Specialization: {CalculatedSpecialization}");

            var previousVisitId = SelectedVisit.VisitId;
            await LoadVisitsForTriageAsync();
            SelectedVisit = RegisteredVisits.FirstOrDefault(visit => visit.VisitId == previousVisitId);
        }
        catch (Exception ex)
        {
            await ShowDialog("Triage Failed", ex.Message);
        }
    }

    [RelayCommand]
    private async Task MoveToQueue()
    {
        if (SelectedVisit == null)
            return;

        try
        {
            await triageService.MoveVisitToQueueAsync(SelectedVisit.VisitId);
            await ShowDialog("Moved to Queue", $"Visit {SelectedVisit.VisitId} is now WAITING_FOR_ROOM.");
            ResetForm();
            await LoadVisitsForTriageAsync();
        }
        catch (Exception ex)
        {
            await ShowDialog("Error", ex.Message);
        }
    }

    [RelayCommand]
    private async Task CancelTriage()
    {
        if (SelectedVisit == null)
        {
            await ShowDialog("No Visit Selected", "Please select a visit from the list before performing triage.");
            return;
        }

        await triageService.CloseVisitAsync(SelectedVisit.VisitId);
        await ShowDialog("Visit closed", $"The visit {SelectedVisit.VisitId} has been closed!");
        await LoadVisitsForTriageAsync();
    }

    private void ResetForm()
    {
        SelectedVisit = null;
        Consciousness = 0;
        Breathing = 0;
        Bleeding = 0;
        InjuryType = 0;
        PainLevel = 0;
        CalculatedSeverity = 0;
        CalculatedSpecialization = string.Empty;
        IsTriaged = false;
        lastTriageResult = null;
        OnPropertyChanged(nameof(SelectedVisitLabel));
        OnPropertyChanged(nameof(TriageResultText));
        OnPropertyChanged(nameof(SpecializationResultText));
    }

    private async Task LoadVisitsForTriageAsync()
    {
        RegisteredVisits.Clear();
        var visits = await triageService.GetVisitsForTriageAsync();
        foreach (var visit in visits)
        {
            RegisteredVisits.Add(visit);
        }

        OnPropertyChanged(nameof(HasRegisteredVisits));
        OnPropertyChanged(nameof(RegisteredVisitsStatusText));
    }

    private async Task ShowDialog(string title, string content)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = "OK",
            XamlRoot = ((App)Application.Current).CurrentWindow?.Content?.XamlRoot,
        };
        await dialog.ShowAsync();
    }
}
