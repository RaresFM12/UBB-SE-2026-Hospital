using Hospital.Data.Models;
using Hospital.Data.Models;
using Hospital.Web.Models.Examination;
using Hospital.Web.Services;
using Hospital.Shared.Proxies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize]
public class ExaminationController : Controller
{
    private readonly IErWorkflowApiClient erApiClient;

    public ExaminationController(IErWorkflowApiClient erApiClient)
    {
        this.erApiClient = erApiClient;
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
            ERVisit visit = await erApiClient.GetVisitAsync(visitId, cancellationToken)
                ?? throw new KeyNotFoundException($"Visit {visitId} was not found.");

            if (!string.Equals(visit.Status, ERVisit.VisitStatus.IN_ROOM, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(visit.Status, ERVisit.VisitStatus.WAITING_FOR_DOCTOR, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] =
                    "Doctor assignment is only available for visits currently in a room.";
                return RedirectToAction(nameof(Index), new { selectedVisitId = visitId });
            }

            Examination assignment = await erApiClient.RequestDoctorAsync(visitId, cancellationToken);

            TempData["SuccessMessage"] =
                $"Doctor {assignment.Doctor.FullName} ({assignment.Doctor.Specialization}) assigned to visit {visitId}.";
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
            ERVisit visit = await erApiClient.GetVisitAsync(form.VisitId, cancellationToken)
                ?? throw new KeyNotFoundException($"Visit {form.VisitId} was not found.");

            if (!string.Equals(visit.Status, ERVisit.VisitStatus.WAITING_FOR_DOCTOR, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(visit.Status, ERVisit.VisitStatus.IN_EXAMINATION, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "The visit must be waiting for a doctor before the examination can be saved.";
                return RedirectToAction(nameof(Index), new { selectedVisitId = form.VisitId });
            }

            await erApiClient.SaveExaminationAsync(new SaveExaminationRequest
            {
                VisitId = form.VisitId,
                Notes = form.Notes,
            }, cancellationToken);

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
            ERExaminationSummary summary = await erApiClient.GetExaminationSummaryAsync(visitId, cancellationToken)
                ?? throw new InvalidOperationException("No examination summary is available for this visit.");

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
        List<ERVisit> eligibleVisits = await erApiClient.GetEligibleExaminationVisitsAsync(cancellationToken);
        ERVisit? selectedVisit = selectedVisitId.HasValue
            ? eligibleVisits.FirstOrDefault(visit => visit.VisitId == selectedVisitId.Value)
                ?? await erApiClient.GetVisitAsync(selectedVisitId.Value, cancellationToken)
            : null;

        var model = new ExaminationViewModel
        {
            SelectedVisitId = selectedVisitId,
            EligibleVisits = eligibleVisits.OrderBy(visit => visit.ArrivalDateTime).Select(MapVisit).ToList(),
            SelectedVisit = selectedVisit is null ? null : MapVisit(selectedVisit),
            Form = form
        };

        if (selectedVisit is null) return model;

        List<Examination> history = await erApiClient.GetPatientExaminationHistoryAsync(
            selectedVisit.Patient.PatientId.ToString(),
            cancellationToken);
        model.ExaminationHistory = history
            .Where(examination => !string.IsNullOrWhiteSpace(examination.Findings))
            .Select(e => new ExaminationHistoryItemViewModel
            {
                ExamId = e.ExaminationId,
                VisitId = e.Visit.VisitId,
                DoctorId = e.Doctor.StaffId,
                ExamTime = e.ExaminationDate,
                RoomId = e.Room.RoomId,
                Notes = e.Findings
            }).ToList();

        Triage? triage = await erApiClient.GetTriageByVisitIdAsync(selectedVisit.VisitId, cancellationToken);
        TriageParameters? triageParameters = triage is null
            ? null
            : await erApiClient.GetTriageParametersByTriageIdAsync(triage.TriageId, cancellationToken);

        if (triage is not null && triageParameters is not null)
        {
            model.TriageDetails = new ExaminationTriageViewModel
            {
                TriageLevel = triage.TriageLevel,
                Specialization = triage.Specialization,
                NurseId = triage.NurseId,
                Consciousness = triageParameters.Consciousness,
                Breathing = triageParameters.Breathing,
                Bleeding = triageParameters.Bleeding,
                InjuryType = triageParameters.InjuryType,
                PainLevel = triageParameters.PainLevel
            };
        }

        Examination? existing = history.FirstOrDefault(evaluation => evaluation.Visit.VisitId == selectedVisit.VisitId);
        bool isInRoom = string.Equals(
            selectedVisit.Status,
            ERVisit.VisitStatus.IN_ROOM,
            StringComparison.OrdinalIgnoreCase);
        bool isWaitingForDoctor = string.Equals(
            selectedVisit.Status,
            ERVisit.VisitStatus.WAITING_FOR_DOCTOR,
            StringComparison.OrdinalIgnoreCase);
        bool isInExamination = string.Equals(
            selectedVisit.Status,
            ERVisit.VisitStatus.IN_EXAMINATION,
            StringComparison.OrdinalIgnoreCase);
        model.CanRequestDoctor = isInRoom || (isWaitingForDoctor && existing is null);
        model.CanSaveExamination = existing is not null && (isWaitingForDoctor || isInExamination);

        if (existing is not null)
        {
            model.Form = new ExaminationFormViewModel
            {
                VisitId = selectedVisit.VisitId,
                DoctorId = existing.Doctor.StaffId,
                Notes = string.IsNullOrWhiteSpace(form.Notes) ? existing.Findings : form.Notes
            };
            model.DoctorName = existing.Doctor.FullName;
            model.DoctorSpecialty = existing.Doctor.Specialization;
        }
        else
        {
            model.Form.VisitId = selectedVisit.VisitId;
            model.Form.DoctorId = form.DoctorId;
            model.Form.Notes = form.Notes;
        }

        return model;
    }

    private async Task<ERRoom> ResolveAssignedRoomAsync(int visitId, CancellationToken cancellationToken)
    {
        ERRoom? currentRoom = (await erApiClient.GetRoomsAsync(cancellationToken))
            .FirstOrDefault(room => room.CurrentVisit?.VisitId == visitId);
        if (currentRoom is not null) return currentRoom;

        Examination? latestExam = (await erApiClient.GetExaminationsByVisitIdAsync(visitId, cancellationToken))
            .OrderByDescending(evaluation => evaluation.ExaminationDate).FirstOrDefault();
        if (latestExam is not null) return latestExam.Room;

        ERRoom? fallbackRoom = (await erApiClient.GetRoomsAsync(cancellationToken)).OrderBy(room => room.RoomId).FirstOrDefault();
        return fallbackRoom ?? throw new InvalidOperationException("No ER rooms are available.");
    }

    private static ExaminationVisitViewModel MapVisit(ERVisit visit) =>
        new()
        {
            VisitId = visit.VisitId,
            PatientId = visit.Patient.Cnp,
            ArrivalTime = visit.ArrivalDateTime,
            ChiefComplaint = visit.ChiefComplaint,
            Status = visit.Status
        };

    private IActionResult RedirectToLogin()
    {
        TempData["ErrorMessage"] = "Please sign in before opening examinations.";
        return RedirectToAction("AuthenticationView", "Authentication");
    }
}

