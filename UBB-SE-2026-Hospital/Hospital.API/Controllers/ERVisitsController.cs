using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Doctor","Nurse","ERDoctor")]
[Route("api/ervisits")]
public class ERVisitsController(
    IERVisitService erVisitService,
    ITriageService triageService,
    ILogger<ERVisitsController> logger) : ControllerBase
{
    public sealed class SaveERVisitRequest
    {
        public int PatientId { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public string ChiefComplaint { get; set; } = string.Empty;
        public string Status { get; set; } = ERVisit.VisitStatus.REGISTERED;
    }

    [HttpGet]
    public async Task<ActionResult<List<ERVisit>>> GetAll()
    {
        try { return Ok(await erVisitService.GetAllAsync()); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch ER visits."); return Problem(statusCode: 500, title: "Could not fetch ER visits."); }
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<ERVisit>>> GetActive()
    {
        try { return Ok(await erVisitService.GetActiveVisitsAsync()); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch active ER visits."); return Problem(statusCode: 500, title: "Could not fetch active ER visits."); }
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<List<ERVisit>>> GetByStatus(string status)
    {
        try { return Ok(await erVisitService.GetByStatusAsync(status)); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch ER visits by status {Status}.", status); return Problem(statusCode: 500, title: "Could not fetch ER visits by status."); }
    }

    [HttpGet("for-triage")]
    public async Task<ActionResult<List<ERVisit>>> GetForTriage()
    {
        try { return Ok(await triageService.GetVisitsForTriageAsync()); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch visits for triage."); return Problem(statusCode: 500, title: "Could not fetch visits for triage."); }
    }

    [HttpGet("patient/{patientId:int}")]
    public async Task<ActionResult<List<ERVisit>>> GetByPatientId(int patientId)
    {
        try { return Ok(await erVisitService.GetByPatientIdAsync(patientId)); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch ER visits for patient {PatientId}.", patientId); return Problem(statusCode: 500, title: "Could not fetch patient ER visits."); }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ERVisit>> GetById(int id)
    {
        try
        {
            ERVisit? result = await erVisitService.GetByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch ER visit {Id}.", id); return Problem(statusCode: 500, title: "Could not fetch ER visit."); }
    }

    [HttpPost]
    public async Task<ActionResult<ERVisit>> Create([FromBody] JsonElement payload)
    {
        try
        {
            SaveERVisitRequest request = ParseVisitRequest(payload);
            var visit = new ERVisit
            {
                Patient = new Patient { PatientId = request.PatientId },
                ArrivalDateTime = request.ArrivalDateTime,
                ChiefComplaint = request.ChiefComplaint,
                Status = request.Status,
            };

            ERVisit result = await erVisitService.CreateAsync(visit);
            return CreatedAtAction(nameof(GetById), new { id = result.VisitId }, result);
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to create ER visit."); return Problem(statusCode: 500, title: "Could not create ER visit."); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] JsonElement payload)
    {
        try
        {
            SaveERVisitRequest request = ParseVisitRequest(payload);
            var visit = new ERVisit
            {
                VisitId = id,
                Patient = new Patient { PatientId = request.PatientId },
                ArrivalDateTime = request.ArrivalDateTime,
                ChiefComplaint = request.ChiefComplaint,
                Status = request.Status,
            };

            visit.VisitId = id;
            await erVisitService.UpdateAsync(visit);
            return NoContent();
        }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (Exception ex) { logger.LogError(ex, "Failed to update ER visit {Id}.", id); return Problem(statusCode: 500, title: "Could not update ER visit."); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            ERVisit? visit = await erVisitService.GetByIdAsync(id);
            if (visit is null) return NotFound();

            await erVisitService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to delete ER visit {Id}.", id); return Problem(statusCode: 500, title: "Could not delete ER visit."); }
    }

    [HttpPost("auto-assign-room")]
    public async Task<ActionResult<bool>> AutoAssignRoom()
    {
        try { return Ok(await erVisitService.AutoAssignHighestPriorityRoomAsync()); }
        catch (Exception ex) { logger.LogError(ex, "Failed to auto-assign ER room."); return Problem(statusCode: 500, title: "Could not auto-assign ER room."); }
    }

    [HttpPost("{visitId:int}/assign-room/{roomId:int}")]
    public async Task<IActionResult> AssignRoom(int visitId, int roomId)
    {
        try { await erVisitService.AssignRoomAsync(visitId, roomId); return NoContent(); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (Exception ex) { logger.LogError(ex, "Failed to assign room {RoomId} to visit {VisitId}.", roomId, visitId); return Problem(statusCode: 500, title: "Could not assign ER room."); }
    }

    [HttpPost("{visitId:int}/transfer")]
    public async Task<IActionResult> Transfer(int visitId)
    {
        try { await erVisitService.TransferVisitAsync(visitId); return NoContent(); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (Exception ex) { logger.LogError(ex, "Failed to transfer ER visit {VisitId}.", visitId); return Problem(statusCode: 500, title: "Could not transfer ER visit."); }
    }

    [HttpPost("{visitId:int}/retry-transfer")]
    public async Task<IActionResult> RetryTransfer(int visitId)
    {
        try { await erVisitService.RetryTransferAsync(visitId); return NoContent(); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (Exception ex) { logger.LogError(ex, "Failed to retry transfer for ER visit {VisitId}.", visitId); return Problem(statusCode: 500, title: "Could not retry ER transfer."); }
    }

    [HttpPost("{visitId:int}/close")]
    public async Task<IActionResult> Close(int visitId)
    {
        try { await erVisitService.CloseVisitAsync(visitId); return NoContent(); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (Exception ex) { logger.LogError(ex, "Failed to close ER visit {VisitId}.", visitId); return Problem(statusCode: 500, title: "Could not close ER visit."); }
    }

    [HttpPost("{visitId:int}/move-to-queue")]
    public async Task<IActionResult> MoveToQueue(int visitId)
    {
        try { await triageService.MoveVisitToQueueAsync(visitId); return NoContent(); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (Exception ex) { logger.LogError(ex, "Failed to move ER visit {VisitId} to queue.", visitId); return Problem(statusCode: 500, title: "Could not move ER visit to queue."); }
    }

    private static SaveERVisitRequest ParseVisitRequest(JsonElement payload)
    {
        return new SaveERVisitRequest
        {
            PatientId = ReadNestedInt(payload, "patient", "patientId") ?? ReadInt(payload, "patientId")
                ?? throw new ArgumentException("Patient id is required."),
            ArrivalDateTime = ReadDateTime(payload, "arrivalDateTime") ?? DateTime.Now,
            ChiefComplaint = ReadString(payload, "chiefComplaint") ?? string.Empty,
            Status = ReadString(payload, "status") ?? ERVisit.VisitStatus.REGISTERED,
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
