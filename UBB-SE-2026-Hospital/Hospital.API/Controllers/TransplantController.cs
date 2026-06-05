using Hospital.Data.Models;
using Hospital.Data.Models;
using Hospital.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;


namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Doctor","Nurse")]
[Route("api/transplants")]
public class TransplantController(ITransplantService transplantService, ILogger<TransplantController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Transplant>>> GetAll()
    {
        try { return Ok(await transplantService.GetAllAsync()); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch transplants."); return Problem(statusCode: 500, title: "Could not fetch transplants."); }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Transplant>> GetById(int id)
    {
        try
        {
            Transplant? result = await transplantService.GetByIdAsync(id);
            if (result is null) return NotFound();
            return Ok(result);
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch transplant {Id}.", id); return Problem(statusCode: 500, title: "Could not fetch transplant."); }
    }

    [HttpPost]
    public async Task<ActionResult<Transplant>> Create([FromBody] Transplant transplant)
    {
        try
        {
            Transplant result = await transplantService.CreateAsync(transplant);
            return CreatedAtAction(nameof(GetById), new { id = result.TransplantId }, result);
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to create transplant."); return Problem(statusCode: 500, title: "Could not create transplant."); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Transplant transplant)
    {
        try
        {
            transplant.TransplantId = id;
            await transplantService.UpdateAsync(transplant);
            return NoContent();
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to update transplant {Id}.", id); return Problem(statusCode: 500, title: "Could not update transplant."); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await transplantService.DeleteAsync(id); return NoContent(); }
        catch (Exception ex) { logger.LogError(ex, "Failed to delete transplant {Id}.", id); return Problem(statusCode: 500, title: "Could not delete transplant."); }
    }

    [HttpGet("patient/{patientId:int}")]
    public async Task<ActionResult<List<Transplant>>> GetByPatientId(int patientId)
    {
        try { return Ok(await transplantService.GetByPatientIdAsync(patientId)); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch transplants for patient {PatientId}.", patientId); return Problem(statusCode: 500, title: "Could not fetch transplants."); }
    }

    [HttpGet("matches/donor/{donorId:int}")]
    public async Task<ActionResult<List<TransplantMatch>>> GetTopMatchesForDonor(int donorId, [FromQuery] string organType)
    {
        try { return Ok(await transplantService.GetTopMatchesAsDisplayModelsAsync(donorId, organType)); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch matches for donor {DonorId}.", donorId); return Problem(statusCode: 500, title: "Could not fetch transplant matches."); }
    }

    [HttpGet("urgent/{patientId:int}")]
    public async Task<ActionResult<bool>> IsUrgent(int patientId)
    {
        try { return Ok(await transplantService.IsUrgentAsync(patientId)); }
        catch (Exception ex) { logger.LogError(ex, "Failed to check urgency for patient {PatientId}.", patientId); return Problem(statusCode: 500, title: "Could not check urgency."); }
    }

    [HttpGet("chronic-warning/{patientId:int}")]
    public async Task<ActionResult<string?>> GetChronicWarning(int patientId)
    {
        try { return Ok(await transplantService.GetChronicWarningAsync(patientId)); }
        catch (Exception ex) { logger.LogError(ex, "Failed to get chronic warning for patient {PatientId}.", patientId); return Problem(statusCode: 500, title: "Could not get chronic warning."); }
    }

    [HttpPost("waitlist")]
    public async Task<ActionResult> CreateWaitlistRequest([FromBody] CreateWaitlistRequest dto)
    {
        try { await transplantService.CreateWaitlistRequestAsync(dto.ReceiverId, dto.OrganType); return Ok(); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (Exception ex) { logger.LogError(ex, "Failed to create waitlist request."); return Problem(statusCode: 500, title: "Could not create waitlist request."); }
    }

    [HttpPut("{id:int}/assign-donor")]
    public async Task<ActionResult> AssignDonor(int id, [FromBody] AssignDonorRequest dto)
    {
        try { await transplantService.AssignDonorAsync(id, dto.DonorId, dto.FinalScore); return Ok(); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (Exception ex) { logger.LogError(ex, "Failed to assign donor for transplant {Id}.", id); return Problem(statusCode: 500, title: "Could not assign donor."); }
    }
}
