using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Doctor","Nurse","ERDoctor")]
[Route("api/examinations")]
public class ExaminationController(IExaminationService examinationService, ILogger<ExaminationController> logger) : ControllerBase
{
    public sealed class SaveExaminationRequest
    {
        public int VisitId { get; set; }
        public int DoctorId { get; set; }
        public int RoomId { get; set; }
        public DateTime ExaminationDate { get; set; }
        public string Findings { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<List<Examination>>> GetAll()
    {
        try { return Ok(await examinationService.GetAllAsync()); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch examinations."); return Problem(statusCode: 500, title: "Could not fetch examinations."); }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Examination>> GetById(int id)
    {
        try
        {
            Examination? result = await examinationService.GetByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch examination {Id}.", id); return Problem(statusCode: 500, title: "Could not fetch examination."); }
    }

    [HttpGet("visit/{visitId:int}")]
    public async Task<ActionResult<List<Examination>>> GetByVisitId(int visitId)
    {
        try { return Ok(await examinationService.GetByVisitIdAsync(visitId)); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch examinations for visit {VisitId}.", visitId); return Problem(statusCode: 500, title: "Could not fetch visit examinations."); }
    }

    [HttpGet("eligible-visits")]
    public async Task<ActionResult<List<ERVisit>>> GetEligibleVisits()
    {
        try { return Ok(await examinationService.GetEligibleVisitsAsync()); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch eligible examination visits."); return Problem(statusCode: 500, title: "Could not fetch eligible examination visits."); }
    }

    [HttpGet("patient-history/{patientId:int}")]
    public async Task<ActionResult<List<Examination>>> GetPatientHistory(int patientId)
    {
        try { return Ok(await examinationService.GetPatientHistoryAsync(patientId)); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch examination history for patient {PatientId}.", patientId); return Problem(statusCode: 500, title: "Could not fetch examination history."); }
    }

    [HttpGet("patient/{patientId:int}")]
    public Task<ActionResult<List<Examination>>> GetPatientHistoryAlias(int patientId)
        => GetPatientHistory(patientId);

    [HttpGet("summary/{visitId:int}")]
    public async Task<ActionResult<ERExaminationSummary>> GetSummary(int visitId)
    {
        try
        {
            ERExaminationSummary? result = await examinationService.GetSummaryByVisitIdAsync(visitId);
            return result is null ? NotFound() : Ok(result);
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch examination summary for visit {VisitId}.", visitId); return Problem(statusCode: 500, title: "Could not fetch examination summary."); }
    }

    [HttpPost]
    public async Task<ActionResult<Examination>> Create([FromBody] JsonElement payload)
    {
        try
        {
            SaveExaminationRequest request = ParseSaveRequest(payload);
            var examination = new Examination
            {
                Visit = new ERVisit { VisitId = request.VisitId },
                Doctor = new Staff { StaffId = request.DoctorId },
                Room = new ERRoom { RoomId = request.RoomId },
                ExaminationDate = request.ExaminationDate == default ? DateTime.Now : request.ExaminationDate,
                Findings = request.Findings,
                Recommendation = request.Recommendation,
            };

            Examination result = await examinationService.CreateAsync(examination);
            return CreatedAtAction(nameof(GetById), new { id = result.ExaminationId }, result);
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to create examination."); return Problem(statusCode: 500, title: "Could not create examination."); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] JsonElement payload)
    {
        try
        {
            SaveExaminationRequest request = ParseSaveRequest(payload);
            var examination = new Examination
            {
                ExaminationId = id,
                Visit = new ERVisit { VisitId = request.VisitId },
                Doctor = new Staff { StaffId = request.DoctorId },
                Room = new ERRoom { RoomId = request.RoomId },
                ExaminationDate = request.ExaminationDate == default ? DateTime.Now : request.ExaminationDate,
                Findings = request.Findings,
                Recommendation = request.Recommendation,
            };

            examination.ExaminationId = id;
            await examinationService.UpdateAsync(examination);
            return NoContent();
        }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (Exception ex) { logger.LogError(ex, "Failed to update examination {Id}.", id); return Problem(statusCode: 500, title: "Could not update examination."); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            Examination? examination = await examinationService.GetByIdAsync(id);
            if (examination is null) return NotFound();

            await examinationService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to delete examination {Id}.", id); return Problem(statusCode: 500, title: "Could not delete examination."); }
    }

    private static SaveExaminationRequest ParseSaveRequest(JsonElement payload)
    {
        int visitId = ReadNestedInt(payload, "visit", "visitId") ?? ReadInt(payload, "visitId")
            ?? throw new ArgumentException("Visit id is required.");
        int doctorId = ReadNestedInt(payload, "doctor", "staffId") ?? ReadInt(payload, "doctorId")
            ?? throw new ArgumentException("Doctor id is required.");
        int roomId = ReadNestedInt(payload, "room", "roomId") ?? ReadInt(payload, "roomId")
            ?? throw new ArgumentException("Room id is required.");

        return new SaveExaminationRequest
        {
            VisitId = visitId,
            DoctorId = doctorId,
            RoomId = roomId,
            ExaminationDate = ReadDateTime(payload, "examinationDate") ?? DateTime.Now,
            Findings = ReadString(payload, "findings") ?? string.Empty,
            Recommendation = ReadString(payload, "recommendation") ?? string.Empty,
        };
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        foreach (var candidate in element.EnumerateObject())
        {
            if (string.Equals(candidate.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                return candidate.Value.ValueKind == JsonValueKind.String ? candidate.Value.GetString() : candidate.Value.GetRawText();
            }
        }

        return null;
    }

    private static int? ReadInt(JsonElement element, string propertyName)
    {
        foreach (var candidate in element.EnumerateObject())
        {
            if (!string.Equals(candidate.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (candidate.Value.ValueKind == JsonValueKind.Number && candidate.Value.TryGetInt32(out int number))
            {
                return number;
            }

            return int.TryParse(candidate.Value.GetString(), out int parsed) ? parsed : null;
        }

        return null;
    }

    private static int? ReadNestedInt(JsonElement element, string objectName, string propertyName)
    {
        foreach (var candidate in element.EnumerateObject())
        {
            if (!string.Equals(candidate.Name, objectName, StringComparison.OrdinalIgnoreCase) || candidate.Value.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            return ReadInt(candidate.Value, propertyName);
        }

        return null;
    }

    private static DateTime? ReadDateTime(JsonElement element, string propertyName)
    {
        foreach (var candidate in element.EnumerateObject())
        {
            if (!string.Equals(candidate.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return candidate.Value.ValueKind == JsonValueKind.String && DateTime.TryParse(candidate.Value.GetString(), out DateTime parsed)
                ? parsed
                : null;
        }

        return null;
    }
}
