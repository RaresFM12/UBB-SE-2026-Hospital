using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[Authorize]
[Route("api/transfer-logs")]
public class TransferLogController(ITransferLogService transferLogService, ILogger<TransferLogController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TransferLog>>> GetAll()
    {
        try { return Ok(await transferLogService.GetAllAsync()); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch transfer logs."); return Problem(statusCode: 500, title: "Could not fetch transfer logs."); }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TransferLog>> GetById(int id)
    {
        try
        {
            TransferLog? result = await transferLogService.GetByIdAsync(id);
            if (result is null) return NotFound();
            return Ok(result);
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch transfer log {Id}.", id); return Problem(statusCode: 500, title: "Could not fetch transfer log."); }
    }

    [HttpGet("visit/{visitId:int}")]
    public async Task<ActionResult<List<TransferLog>>> GetByVisitId(int visitId)
    {
        try { return Ok(await transferLogService.GetByVisitIdAsync(visitId)); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch transfer logs for visit {VisitId}.", visitId); return Problem(statusCode: 500, title: "Could not fetch transfer logs."); }
    }

    [HttpGet("eligible-visits")]
    public async Task<ActionResult<List<ERTransferEligibleVisit>>> GetEligibleVisits()
    {
        try { return Ok(await transferLogService.GetEligibleVisitsAsync()); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch eligible visits."); return Problem(statusCode: 500, title: "Could not fetch eligible visits."); }
    }

    [HttpPost]
    public async Task<ActionResult<TransferLog>> Create([FromBody] TransferLog transferLog)
    {
        try
        {
            TransferLog result = await transferLogService.CreateAsync(transferLog);
            return CreatedAtAction(nameof(GetById), new { id = result.TransferLogId }, result);
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to create transfer log."); return Problem(statusCode: 500, title: "Could not create transfer log."); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TransferLog transferLog)
    {
        try
        {
            transferLog.TransferLogId = id;
            await transferLogService.UpdateAsync(transferLog);
            return NoContent();
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to update transfer log {Id}.", id); return Problem(statusCode: 500, title: "Could not update transfer log."); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await transferLogService.DeleteAsync(id); return NoContent(); }
        catch (Exception ex) { logger.LogError(ex, "Failed to delete transfer log {Id}.", id); return Problem(statusCode: 500, title: "Could not delete transfer log."); }
    }
}
