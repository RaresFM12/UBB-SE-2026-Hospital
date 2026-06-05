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
    private readonly IERVisitService erVisitService;
    private readonly IERRoomService erRoomService;
    private readonly ITriageService triageService;
    private readonly IERDispatchService dispatchService;

    public Microsoft.UI.Xaml.XamlRoot? XamlRoot { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RequestDoctorCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveExaminationCommand))]
    private ERVisit? selectedVisit;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveExaminationCommand))]
    private int doctorId;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveExaminationCommand))]
    private string notes = string.Empty;

    [ObservableProperty] private string doctorName = string.Empty;
    [ObservableProperty] private string doctorSpecialty = string.Empty;
    [ObservableProperty] private ObservableCollection<ERVisit> eligibleVisits = new ObservableCollection<ERVisit>();
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private ObservableCollection<Examination> examinationHistory = new ObservableCollection<Examination>();
    [ObservableProperty] private string triageLevelDisplay = string.Empty;
    [ObservableProperty] private string triageSpecialization = string.Empty;
    [ObservableProperty] private string triageNurseId = string.Empty;
    [ObservableProperty] private string savedTimeDisplay = string.Empty;

    public ExaminationViewModel(
        IExaminationService examinationService,
        IERVisitService erVisitService,
        IERRoomService erRoomService,
        ITriageService triageService,
        IERDispatchService dispatchService)
    {
        this.examinationService = examinationService;
        this.erVisitService = erVisitService;
        this.erRoomService = erRoomService;
        this.triageService = triageService;
        this.dispatchService = dispatchService;
    }

    private bool CanRequestDoctor()
        => SelectedVisit != null && SelectedVisit.Status == ERVisit.VisitStatus.IN_ROOM;

    private bool CanSaveExamination()
    {
        return SelectedVisit != null &&
               DoctorId != 0 &&
               !string.IsNullOrWhiteSpace(Notes) &&
               (SelectedVisit.Status == ERVisit.VisitStatus.WAITING_FOR_DOCTOR ||
                SelectedVisit.Status == ERVisit.VisitStatus.IN_EXAMINATION);
    }

    [RelayCommand]
    public async Task LoadData()
    {
        EligibleVisits.Clear();
        SelectedVisit = null;
        DoctorId = 0;
        DoctorName = string.Empty;
        DoctorSpecialty = string.Empty;
        Notes = string.Empty;
        StatusMessage = string.Empty;
        ClearTriageDetails();

        var eligible = await examinationService.GetEligibleVisitsAsync();
        foreach (var visit in eligible)
        {
            EligibleVisits.Add(visit);
        }

        StatusMessage = EligibleVisits.Count == 0
            ? "No visits are available in the examination workspace yet. Visits appear here when they are in room, waiting for a doctor, or already in examination."
            : $"{EligibleVisits.Count} visit(s) are available in the examination workspace, including ongoing examinations.";
    }

    partial void OnSelectedVisitChanged(ERVisit? value)
        => _ = HandleSelectedVisitChangedAsync(value);

    private async Task HandleSelectedVisitChangedAsync(ERVisit? value)
    {
        if (value == null)
        {
            DoctorId = 0; DoctorName = string.Empty; DoctorSpecialty = string.Empty;
            Notes = string.Empty; ClearTriageDetails(); ExaminationHistory.Clear();
            return;
        }

        ExaminationHistory.Clear();
        var history = await examinationService.GetPatientHistoryAsync(value.Patient?.PatientId ?? 0);
        foreach (var exam in history)
        {
            ExaminationHistory.Add(exam);
        }

        try
        {
            await LoadTriageDetailsAsync(value.VisitId);
        }
        catch { ClearTriageDetails(); }

        if (value.Status == ERVisit.VisitStatus.WAITING_FOR_DOCTOR || value.Status == ERVisit.VisitStatus.IN_EXAMINATION)
        {
            var existingExam = history.FirstOrDefault(e => e.Visit?.VisitId == value.VisitId);
            if (existingExam != null)
            {
                DoctorId = existingExam.Doctor?.StaffId ?? 0;
                DoctorName = $"Staff #{existingExam.Doctor?.StaffId}";
                DoctorSpecialty = existingExam.Doctor?.Specialization ?? "General";
                Notes = existingExam.Findings ?? string.Empty;
            }
            else
            {
                var request = await dispatchService.GetRequestByVisitIdAsync(value.VisitId);
                if (ApplyDispatchAssignment(request))
                {
                    StatusMessage = request?.Status == "ASSIGNED"
                        ? $"Doctor assigned through ER Dispatch for visit {value.VisitId}."
                        : $"Doctor request for visit {value.VisitId} is pending in ER Dispatch.";
                }
                else
                {
                    var triage = await triageService.GetByVisitIdAsync(value.VisitId);
                    if (triage != null && !string.IsNullOrEmpty(triage.Specialization))
                    {
                        DoctorName = $"Pending: {triage.Specialization}";
                        DoctorSpecialty = triage.Specialization;
                    }
                }
            }
        }

        if (!history.Any(e => e.Visit?.VisitId == value.VisitId))
        {
            Notes = string.Empty;
        }
    }

    private async Task LoadTriageDetailsAsync(int visitId)
    {
        var triage = await triageService.GetByVisitIdAsync(visitId);
        if (triage == null)
        {
            ClearTriageDetails();
            return;
        }
        TriageLevelDisplay = $"Level {triage.TriageLevel}";
        TriageSpecialization = string.IsNullOrEmpty(triage.Specialization) ? "N/A" : triage.Specialization;
        TriageNurseId = $"Nurse #{triage.NurseId}";
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
        if (SelectedVisit == null) return;

        try
        {
            var triage = await triageService.GetByVisitIdAsync(SelectedVisit.VisitId);
            if (triage == null || string.IsNullOrWhiteSpace(triage.Specialization))
            {
                await ShowDialog("Missing Triage", "This visit must have triage information and a target specialization before requesting a doctor.");
                return;
            }

            var requestId = await dispatchService.CreateRequestAsync(triage.Specialization, "ER", SelectedVisit.VisitId);
            StatusMessage = $"Doctor request #{requestId} was sent to ER Dispatch for {triage.Specialization}.";
            await ShowDialog("Doctor Requested",
                $"Visit {SelectedVisit.VisitId} was sent to ER Dispatch.\nRequired specialization: {triage.Specialization}\nRequest ID: {requestId}");
            await LoadData();
        }
        catch (Exception ex)
        {
            await ShowDialog("Error", $"Failed to request doctor: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveExamination))]
    public async Task SaveExamination()
    {
        if (SelectedVisit == null) return;
        try
        {
            int assignedRoomId = await ResolveAssignedRoomIdAsync(SelectedVisit.VisitId);
            var examination = new Examination
            {
                Visit = SelectedVisit,
                Doctor = new Staff { StaffId = DoctorId },
                ExaminationDate = DateTime.Now,
                Findings = Notes,
                Room = assignedRoomId > 0 ? new ERRoom { RoomId = assignedRoomId } : null!,
            };

            await examinationService.CreateAsync(examination);

            await erVisitService.UpdateAsync(new ERVisit
            {
                VisitId = SelectedVisit.VisitId,
                Status = ERVisit.VisitStatus.IN_EXAMINATION,
                Patient = SelectedVisit.Patient,
                ChiefComplaint = SelectedVisit.ChiefComplaint,
                ArrivalDateTime = SelectedVisit.ArrivalDateTime,
            });

            StatusMessage = $"Examination saved for visit {SelectedVisit.VisitId}.";

            await ShowDialog("Examination Saved",
                $"Examination for Visit {SelectedVisit.VisitId} has been saved.\nDoctor: {DoctorName} ({DoctorSpecialty})");
            await LoadData();
        }
        catch (Exception ex)
        {
            await ShowDialog("Error", $"Failed to save examination: {ex.Message}");
        }
    }

    private async Task<int> ResolveAssignedRoomIdAsync(int visitId)
    {
        var rooms = await erRoomService.GetAllAsync();
        var currentRoom = rooms.FirstOrDefault(r => r.CurrentVisit?.VisitId == visitId);
        if (currentRoom != null) return currentRoom.RoomId;

        var exams = await examinationService.GetByVisitIdAsync(visitId);
        var latestExam = exams.OrderByDescending(e => e.ExaminationDate).FirstOrDefault();
        if (latestExam?.Room != null) return latestExam.Room.RoomId;

        var fallbackRoom = rooms.OrderBy(r => r.RoomId).FirstOrDefault();
        return fallbackRoom?.RoomId ?? throw new InvalidOperationException("No ER rooms available.");
    }

    private bool ApplyDispatchAssignment(Hospital.Data.Models.ERRequest? request)
    {
        if (request?.AssignedDoctor == null)
        {
            DoctorId = 0;
            return false;
        }

        DoctorId = request.AssignedDoctor.StaffID;
        DoctorName = string.IsNullOrWhiteSpace(request.AssignedDoctor.FullName)
            ? $"Dr. #{request.AssignedDoctor.StaffID}"
            : request.AssignedDoctor.FullName;
        DoctorSpecialty = string.IsNullOrWhiteSpace(request.AssignedDoctor.Specialization)
            ? "General"
            : request.AssignedDoctor.Specialization;
        return true;
    }

    private async Task ShowDialog(string title, string message)
    {
        if (XamlRoot == null) return;
        var dialog = new ContentDialog { Title = title, Content = message, CloseButtonText = "OK", XamlRoot = XamlRoot };
        await dialog.ShowAsync();
    }
}
