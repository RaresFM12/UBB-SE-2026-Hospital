using Hospital.Data.Models;
using Hospital.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[Authorize]
[Route("api/triage")]
public class TriageController(
    ITriageService triageService,
    ITriageDecisionService triageDecisionService,
    ILogger<TriageController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Triage>>> GetAll()
    {
        try { return Ok(await triageService.GetAllAsync()); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch triage records."); return Problem(statusCode: 500, title: "Could not fetch triage records."); }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Triage>> GetById(int id)
    {
        try
        {
            Triage? result = await triageService.GetByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch triage {Id}.", id); return Problem(statusCode: 500, title: "Could not fetch triage."); }
    }

    [HttpPost]
    public async Task<ActionResult<Triage>> Create([FromBody] Triage triage)
    {
        try
        {
            Triage result = await triageService.CreateAsync(triage);
            return CreatedAtAction(nameof(GetById), new { id = result.TriageId }, result);
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to create triage."); return Problem(statusCode: 500, title: "Could not create triage."); }
    }

    [HttpPost("decide")]
    public ActionResult<TriageDecisionResponse> Decide([FromBody] TriageParameters parameters)
    {
        try
        {
            int triageLevel = triageDecisionService.CalculateTriageLevel(parameters);
            string specialization = triageDecisionService.DetermineSpecialization(parameters);
            return Ok(new TriageDecisionResponse(triageLevel, specialization));
        }
        catch (ArgumentOutOfRangeException ex) { return BadRequest(ex.Message); }
        catch (Exception ex) { logger.LogError(ex, "Failed to calculate triage decision."); return Problem(statusCode: 500, title: "Could not calculate triage decision."); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Triage triage)
    {
        try
        {
            triage.TriageId = id;
            await triageService.UpdateAsync(triage);
            return NoContent();
        }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (Exception ex) { logger.LogError(ex, "Failed to update triage {Id}.", id); return Problem(statusCode: 500, title: "Could not update triage."); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            Triage? triage = await triageService.GetByIdAsync(id);
            if (triage is null) return NotFound();

            await triageService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to delete triage {Id}.", id); return Problem(statusCode: 500, title: "Could not delete triage."); }
    }

    public record TriageDecisionResponse(int TriageLevel, string Specialization);
}
