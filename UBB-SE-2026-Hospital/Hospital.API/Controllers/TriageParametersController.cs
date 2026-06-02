#if false
using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Doctor","Nurse","ERDoctor")]
[Route("api/triageparameters")]
public class TriageParametersController(
    ITriageParametersService triageParametersService,
    ILogger<TriageParametersController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TriageParameters>>> GetAll()
    {
        try { return Ok(await triageParametersService.GetAllAsync()); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch triage parameters."); return Problem(statusCode: 500, title: "Could not fetch triage parameters."); }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TriageParameters>> GetById(int id)
    {
        try
        {
            TriageParameters? result = await triageParametersService.GetByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch triage parameters {Id}.", id); return Problem(statusCode: 500, title: "Could not fetch triage parameters."); }
    }

    [HttpGet("triage/{triageId:int}")]
    public async Task<ActionResult<TriageParameters>> GetByTriageId(int triageId)
    {
        try
        {
            TriageParameters? result = await triageParametersService.GetByTriageIdAsync(triageId);
            return result is null ? NotFound() : Ok(result);
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch parameters for triage {TriageId}.", triageId); return Problem(statusCode: 500, title: "Could not fetch triage parameters."); }
    }

    [HttpPost]
    public async Task<ActionResult<TriageParameters>> Create([FromBody] TriageParameters parameters)
    {
        try
        {
            TriageParameters result = await triageParametersService.CreateAsync(parameters);
            return CreatedAtAction(nameof(GetById), new { id = result.TriageParametersId }, result);
        }
        catch (ArgumentOutOfRangeException ex) { return BadRequest(ex.Message); }
        catch (Exception ex) { logger.LogError(ex, "Failed to create triage parameters."); return Problem(statusCode: 500, title: "Could not create triage parameters."); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TriageParameters parameters)
    {
        try
        {
            parameters.TriageParametersId = id;
            await triageParametersService.UpdateAsync(parameters);
            return NoContent();
        }
        catch (ArgumentOutOfRangeException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (Exception ex) { logger.LogError(ex, "Failed to update triage parameters {Id}.", id); return Problem(statusCode: 500, title: "Could not update triage parameters."); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            TriageParameters? parameters = await triageParametersService.GetByIdAsync(id);
            if (parameters is null) return NotFound();

            await triageParametersService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to delete triage parameters {Id}.", id); return Problem(statusCode: 500, title: "Could not delete triage parameters."); }
    }
}
#endif
