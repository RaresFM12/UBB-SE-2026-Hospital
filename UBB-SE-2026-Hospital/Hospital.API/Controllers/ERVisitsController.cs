using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Doctor","Nurse","ERDoctor")]
[Route("api/ervisits")]
public class ERVisitsController(IERVisitService erVisitService, ITriageService triageService, ILogger<ERVisitsController> logger) : ControllerBase
{
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
    public async Task<ActionResult<ERVisit>> Create([FromBody] ERVisit visit)
    {
        try
        {
            ERVisit result = await erVisitService.CreateAsync(visit);
            return CreatedAtAction(nameof(GetById), new { id = result.VisitId }, result);
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to create ER visit."); return Problem(statusCode: 500, title: "Could not create ER visit."); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ERVisit visit)
    {
        try
        {
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

    [HttpGet("for-triage")]
    public async Task<ActionResult<List<ERVisit>>> GetForTriage()
    {
        try { return Ok(await triageService.GetVisitsForTriageAsync()); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch visits for triage."); return Problem(statusCode: 500, title: "Could not fetch visits for triage."); }
    }

    [HttpPost("{visitId:int}/move-to-queue")]
    public async Task<IActionResult> MoveToQueue(int visitId)
    {
        try { await triageService.MoveVisitToQueueAsync(visitId); return NoContent(); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (Exception ex) { logger.LogError(ex, "Failed to move visit {VisitId} to queue.", visitId); return Problem(statusCode: 500, title: "Could not move visit to queue."); }
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
}
