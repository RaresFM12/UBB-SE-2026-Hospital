using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hospital.Data.Models;
using Hospital.Shared.Services;
using Microsoft.UI.Xaml.Controls;

namespace Hospital.Desktop.ViewModels.ER;

public partial class ExaminationViewModel : ObservableObject
{
    private readonly IExaminationService examinationService;
    private readonly ITriageService triageService;

    public Microsoft.UI.Xaml.XamlRoot? XamlRoot { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RequestDoctorCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveExaminationCommand))]
    [NotifyCanExecuteChangedFor(nameof(ViewSummaryCommand))]
    private ERVisit? selectedVisit;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RequestDoctorCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveExaminationCommand))]
    private int doctorId;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveExaminationCommand))]
    private string notes = string.Empty;

    [ObservableProperty] private string doctorName = string.Empty;
    [ObservableProperty] private string doctorSpecialty = string.Empty;
    [ObservableProperty] private ObservableCollection<ERVisit> eligibleVisits = [];
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private ObservableCollection<Examination> examinationHistory = [];
    [ObservableProperty] private string triageLevelDisplay = string.Empty;
    [ObservableProperty] private string triageSpecialization = string.Empty;
    [ObservableProperty] private string triageNurseId = string.Empty;

    public ExaminationViewModel(
        IExaminationService examinationService,
        ITriageService triageService)
    {
        this.examinationService = examinationService;
        this.triageService = triageService;
    }

    private bool CanRequestDoctor()
        => SelectedVisit is not null
           && DoctorId == 0
           && (SelectedVisit.Status == ERVisit.VisitStatus.IN_ROOM
               || SelectedVisit.Status == ERVisit.VisitStatus.WAITING_FOR_DOCTOR);

    private bool CanSaveExamination()
        => SelectedVisit is not null
           && DoctorId != 0
           && !string.IsNullOrWhiteSpace(Notes)
           && (SelectedVisit.Status == ERVisit.VisitStatus.WAITING_FOR_DOCTOR
               || SelectedVisit.Status == ERVisit.VisitStatus.IN_EXAMINATION);

    private bool CanViewSummary()
        => SelectedVisit?.Status == ERVisit.VisitStatus.IN_EXAMINATION;

    [RelayCommand]
    public async Task LoadData()
    {
        int? selectedVisitId = SelectedVisit?.VisitId;
        EligibleVisits = new ObservableCollection<ERVisit>(
            await examinationService.GetEligibleVisitsAsync());
        SelectedVisit = selectedVisitId is null
            ? null
            : EligibleVisits.FirstOrDefault(visit => visit.VisitId == selectedVisitId);

        StatusMessage = EligibleVisits.Count == 0
            ? "No visits are available for examination."
            : $"{EligibleVisits.Count} visit(s) are available for examination.";
    }

    partial void OnSelectedVisitChanged(ERVisit? value)
        => _ = HandleSelectedVisitChangedAsync(value);

    private async Task HandleSelectedVisitChangedAsync(ERVisit? visit)
    {
        DoctorId = 0;
        DoctorName = string.Empty;
        DoctorSpecialty = string.Empty;
        Notes = string.Empty;
        ExaminationHistory.Clear();

        if (visit is null)
        {
            ClearTriageDetails();
            return;
        }

        try
        {
            Triage? triage = await triageService.GetByVisitIdAsync(visit.VisitId);
            if (triage is null)
            {
                ClearTriageDetails();
            }
            else
            {
                TriageLevelDisplay = $"Level {triage.TriageLevel}";
                TriageSpecialization = string.IsNullOrWhiteSpace(triage.Specialization)
                    ? "N/A"
                    : triage.Specialization;
                TriageNurseId = $"Nurse #{triage.NurseId}";
            }

            int patientId = visit.Patient?.PatientId ?? 0;
            if (patientId != 0)
            {
                foreach (Examination examination in await examinationService.GetPatientHistoryAsync(patientId))
                {
                    if (!string.IsNullOrWhiteSpace(examination.Findings))
                    {
                        ExaminationHistory.Add(examination);
                    }
                }
            }

            Examination? assignment = (await examinationService.GetByVisitIdAsync(visit.VisitId))
                .OrderByDescending(examination => examination.ExaminationDate)
                .FirstOrDefault();
            ApplyAssignment(assignment);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Examination details could not be loaded: {ex.Message}";
        }
    }

    private void ApplyAssignment(Examination? examination)
    {
        if (examination?.Doctor is null)
        {
            return;
        }

        DoctorId = examination.Doctor.StaffId;
        DoctorName = string.IsNullOrWhiteSpace(examination.Doctor.FullName)
            ? $"Doctor #{DoctorId}"
            : examination.Doctor.FullName;
        DoctorSpecialty = string.IsNullOrWhiteSpace(examination.Doctor.Specialization)
            ? "General"
            : examination.Doctor.Specialization;
        Notes = examination.Findings ?? string.Empty;
    }

    private void ClearTriageDetails()
    {
        TriageLevelDisplay = string.Empty;
        TriageSpecialization = string.Empty;
        TriageNurseId = string.Empty;
    }

    [RelayCommand(CanExecute = nameof(CanRequestDoctor))]
    public async Task RequestDoctor()
    {
        if (SelectedVisit is null)
        {
            return;
        }

        try
        {
            Examination assignment = await examinationService.RequestDoctorAsync(SelectedVisit.VisitId);
            ApplyAssignment(assignment);
            SelectedVisit.Status = ERVisit.VisitStatus.WAITING_FOR_DOCTOR;
            OnPropertyChanged(nameof(SelectedVisit));
            RequestDoctorCommand.NotifyCanExecuteChanged();
            SaveExaminationCommand.NotifyCanExecuteChanged();

            StatusMessage = $"Doctor {DoctorName} was assigned to visit {SelectedVisit.VisitId}.";
            await ShowDialog(
                "Doctor Assigned",
                $"{DoctorName} ({DoctorSpecialty}) was assigned to visit {SelectedVisit.VisitId}.");
        }
        catch (Exception ex)
        {
            await ShowDialog("Doctor Request Failed", ex.Message);
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveExamination))]
    public async Task SaveExamination()
    {
        if (SelectedVisit is null)
        {
            return;
        }

        try
        {
            Examination saved = await examinationService.SaveExaminationAsync(
                SelectedVisit.VisitId,
                Notes);
            ApplyAssignment(saved);
            SelectedVisit.Status = ERVisit.VisitStatus.IN_EXAMINATION;
            OnPropertyChanged(nameof(SelectedVisit));
            ViewSummaryCommand.NotifyCanExecuteChanged();
            StatusMessage = $"Examination saved for visit {SelectedVisit.VisitId}.";

            await HandleSelectedVisitChangedAsync(SelectedVisit);
            await ShowDialog("Examination Saved", "The examination was saved and the doctor is available again.");
        }
        catch (Exception ex)
        {
            await ShowDialog("Examination Save Failed", ex.Message);
        }
    }

    [RelayCommand(CanExecute = nameof(CanViewSummary))]
    public async Task ViewSummary()
    {
        if (SelectedVisit is null)
        {
            return;
        }

        try
        {
            ERExaminationSummary? summary =
                await examinationService.GetSummaryByVisitIdAsync(SelectedVisit.VisitId);
            if (summary is null)
            {
                await ShowDialog("Summary Unavailable", "No completed examination summary is available.");
                return;
            }

            string content =
                $"Patient: {summary.FirstName} {summary.LastName}\n" +
                $"Arrival: {summary.ArrivalDateTime:g}\n" +
                $"Complaint: {summary.ChiefComplaint}\n" +
                $"Triage: Level {summary.TriageLevel}, {summary.Specialization}\n" +
                $"Parameters: C:{summary.Consciousness} Br:{summary.Breathing} Bl:{summary.Bleeding} " +
                $"Inj:{summary.InjuryType} Pn:{summary.PainLevel}\n" +
                $"Severity score: {summary.SeverityScore}\n" +
                $"Doctor: {summary.AssignedDoctorName} (ID {summary.DoctorId})\n" +
                $"Exam time: {summary.ExamTime:g}\n\n" +
                $"Notes:\n{summary.Notes}";
            await ShowDialog("Examination Summary", content);
        }
        catch (Exception ex)
        {
            await ShowDialog("Summary Load Failed", ex.Message);
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
