using Hospital.Data.Models.DTOs;
using Hospital.Web.Models.Examination;
using Hospital.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hospital.Web.Services;
using Hospital.Data.Models;

namespace Hospital.Web.Controllers;

[Authorize]
public class ExaminationController : Controller
{
    private readonly IExaminationService examinationService;
    private readonly IERVisitService erVisitService;
    private readonly ITriageService triageService;
    private readonly IErStaffService erStaffService;

    public ExaminationController(
        IExaminationService examinationService,
        IERVisitService erVisitService,
        ITriageService triageService,
        IErStaffService erStaffService)
    {
        this.examinationService = examinationService;
        this.erVisitService = erVisitService;
        this.triageService = triageService;
        this.erStaffService = erStaffService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? selectedVisitId, CancellationToken cancellationToken)
    {
        try
        {
            ExaminationViewModel model = await BuildModelAsync(selectedVisitId, new ExaminationFormViewModel(), cancellationToken);
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            return View(new ExaminationViewModel { ErrorMessage = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestDoctor(int visitId, CancellationToken cancellationToken)
    {
        try
        {
            ER_Visit visit = await erVisitService.GetByIdAsync(visitId)
                ?? throw new KeyNotFoundException($"Visit {visitId} was not found.");

            if (!string.Equals(visit.Status, ER_Visit.VisitStatus.IN_ROOM, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Doctor assignment is only available for visits currently in a room.";
                return RedirectToAction(nameof(Index), new { selectedVisitId = visitId });
            }

            Triage triage = await triageService.GetByVisitIdAsync(visitId)
                ?? throw new InvalidOperationException($"Triage was not found for visit {visitId}.");
            Triage_Parameters parameters = await triageService.GetParametersByTriageIdAsync(triage.Triage_ID)
                ?? throw new InvalidOperationException($"Triage parameters were not found for triage {triage.Triage_ID}.");

            ErDoctorAssignment doctor = erStaffService.RequestDoctor(triage.Specialization, parameters);

            visit.Status = ER_Visit.VisitStatus.WAITING_FOR_DOCTOR;
            await erVisitService.UpdateAsync(visitId, visit);

            TempData["SuccessMessage"] = $"Doctor {doctor.name} ({doctor.specialty}) assigned to visit {visitId}.";
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { selectedVisitId = visitId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        [Bind(Prefix = "Form")] ExaminationFormViewModel form,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ExaminationViewModel invalidModel = await BuildModelAsync(form.VisitId, form, cancellationToken);
            return View("Index", invalidModel);
        }

        try
        {
            ER_Visit visit = await erVisitService.GetByIdAsync(form.VisitId)
                ?? throw new KeyNotFoundException($"Visit {form.VisitId} was not found.");

            if (!string.Equals(visit.Status, ER_Visit.VisitStatus.WAITING_FOR_DOCTOR, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(visit.Status, ER_Visit.VisitStatus.IN_EXAMINATION, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "The visit must be waiting for a doctor before the examination can be saved.";
                return RedirectToAction(nameof(Index), new { selectedVisitId = form.VisitId });
            }

            int roomId = await ResolveAssignedRoomIdAsync(form.VisitId);
            List<Examination> visitExaminations = await examinationService.GetByVisitIdAsync(form.VisitId);
            Examination? existing = visitExaminations.OrderByDescending(e => e.Exam_Time).FirstOrDefault();

            if (existing is null)
            {
                await examinationService.CreateAsync(new Examination
                {
                    Visit_ID = form.VisitId,
                    Doctor_ID = form.DoctorId,
                    Exam_Time = DateTime.Now,
                    Room_ID = roomId,
                    Notes = form.Notes.Trim()
                });
            }
            else
            {
                existing.Doctor_ID = form.DoctorId;
                existing.Room_ID = roomId;
                existing.Notes = form.Notes.Trim();
                await examinationService.UpdateAsync(existing.Exam_ID, existing);
            }

            visit.Status = ER_Visit.VisitStatus.IN_EXAMINATION;
            await erVisitService.UpdateAsync(form.VisitId, visit);

            TempData["SuccessMessage"] = $"Examination for visit {form.VisitId} was saved.";
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { selectedVisitId = form.VisitId });
    }

    [HttpGet]
    public async Task<IActionResult> Summary(int visitId, CancellationToken cancellationToken)
    {
        try
        {
            ERExaminationSummaryDto summary = await examinationService.GetSummaryAsync(visitId)
                ?? throw new InvalidOperationException("No examination summary is available for this visit.");

            ErDoctorAssignment doctor = erStaffService.GetDoctorById(summary.DoctorId);
            summary.AssignedDoctorName = $"{doctor.name} ({doctor.specialty})";

            return View(new ExaminationSummaryViewModel
            {
                VisitId = visitId,
                Summary = summary
            });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToLogin();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index), new { selectedVisitId = visitId });
        }
    }

    private async Task<ExaminationViewModel> BuildModelAsync(
        int? selectedVisitId,
        ExaminationFormViewModel form,
        CancellationToken cancellationToken)
    {
        List<ER_Visit> eligibleVisits = await examinationService.GetEligibleVisitsAsync();
        ER_Visit? selectedVisit = selectedVisitId.HasValue
            ? eligibleVisits.FirstOrDefault(visit => visit.Visit_ID == selectedVisitId.Value)
                ?? await erVisitService.GetByIdAsync(selectedVisitId.Value)
            : null;

        var model = new ExaminationViewModel
        {
            SelectedVisitId = selectedVisitId,
            EligibleVisits = eligibleVisits.OrderBy(visit => visit.Arrival_date_time).Select(MapVisit).ToList(),
            SelectedVisit = selectedVisit is null ? null : MapVisit(selectedVisit),
            Form = form
        };

        if (selectedVisit is null) return model;

        model.CanRequestDoctor = string.Equals(selectedVisit.Status, ER_Visit.VisitStatus.IN_ROOM, StringComparison.OrdinalIgnoreCase);
        model.CanSaveExamination =
            string.Equals(selectedVisit.Status, ER_Visit.VisitStatus.WAITING_FOR_DOCTOR, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(selectedVisit.Status, ER_Visit.VisitStatus.IN_EXAMINATION, StringComparison.OrdinalIgnoreCase);

        List<Examination> history = await examinationService.GetHistoryByPatientIdAsync(selectedVisit.Patient_ID);
        model.ExaminationHistory = history
            .Select(e => new ExaminationHistoryItemViewModel
            {
                ExamId = e.Exam_ID,
                VisitId = e.Visit_ID,
                DoctorId = e.Doctor_ID,
                ExamTime = e.Exam_Time,
                RoomId = e.Room_ID,
                Notes = e.Notes
            }).ToList();

        Triage? triage = await triageService.GetByVisitIdAsync(selectedVisit.Visit_ID);
        Triage_Parameters? triageParameters = triage is null ? null : await triageService.GetParametersByTriageIdAsync(triage.Triage_ID);

        if (triage is not null && triageParameters is not null)
        {
            model.TriageDetails = new ExaminationTriageViewModel
            {
                TriageLevel = triage.Triage_Level,
                Specialization = triage.Specialization,
                NurseId = triage.Nurse_ID,
                Consciousness = triageParameters.Consciousness,
                Breathing = triageParameters.Breathing,
                Bleeding = triageParameters.Bleeding,
                InjuryType = triageParameters.Injury_Type,
                PainLevel = triageParameters.Pain_Level
            };
        }

        Examination? existing = history.FirstOrDefault(e => e.Visit_ID == selectedVisit.Visit_ID);
        if (existing is not null)
        {
            ErDoctorAssignment doctor = erStaffService.GetDoctorById(existing.Doctor_ID);
            model.Form = new ExaminationFormViewModel
            {
                VisitId = selectedVisit.Visit_ID,
                DoctorId = existing.Doctor_ID,
                Notes = string.IsNullOrWhiteSpace(form.Notes) ? existing.Notes : form.Notes
            };
            model.DoctorName = doctor.name;
            model.DoctorSpecialty = doctor.specialty;
        }
        else
        {
            ErDoctorAssignment? doctor = model.TriageDetails is null
                ? null
                : erStaffService.RequestDoctor(model.TriageDetails.Specialization, new Triage_Parameters
                {
                    Consciousness = model.TriageDetails.Consciousness,
                    Breathing = model.TriageDetails.Breathing,
                    Bleeding = model.TriageDetails.Bleeding,
                    Injury_Type = model.TriageDetails.InjuryType,
                    Pain_Level = model.TriageDetails.PainLevel
                });

            model.Form.VisitId = selectedVisit.Visit_ID;
            model.Form.DoctorId = form.DoctorId == 0 ? doctor?.doctorId ?? 0 : form.DoctorId;
            model.Form.Notes = form.Notes;
            model.DoctorName = doctor?.name ?? string.Empty;
            model.DoctorSpecialty = doctor?.specialty ?? string.Empty;
        }

        return model;
    }

    private async Task<int> ResolveAssignedRoomIdAsync(int visitId)
    {
        ER_Room? currentRoom = (await examinationService.GetRoomsAsync())
            .FirstOrDefault(room => room.Current_Visit_ID == visitId);
        if (currentRoom is not null) return currentRoom.Room_ID;

        Examination? latestExam = (await examinationService.GetByVisitIdAsync(visitId))
            .OrderByDescending(e => e.Exam_Time).FirstOrDefault();
        if (latestExam is not null) return latestExam.Room_ID;

        ER_Room? fallbackRoom = (await examinationService.GetRoomsAsync()).OrderBy(room => room.Room_ID).FirstOrDefault();
        return fallbackRoom?.Room_ID ?? throw new InvalidOperationException("No ER rooms are available.");
    }

    private static ExaminationVisitViewModel MapVisit(ER_Visit visit) =>
        new()
        {
            VisitId = visit.Visit_ID,
            PatientId = visit.Patient_ID,
            ArrivalTime = visit.Arrival_date_time,
            ChiefComplaint = visit.Chief_Complaint,
            Status = visit.Status
        };

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before opening examinations.";
        return RedirectToAction("AuthenticationView", "Authentication");
    }
}