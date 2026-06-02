using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Doctor","Nurse","ERDoctor")]
[Route("api/examinations")]
public class ExaminationController(IExaminationService examinationService, ILogger<ExaminationController> logger) : ControllerBase
{
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
    public async Task<ActionResult<Examination>> Create([FromBody] Examination examination)
    {
        try
        {
            Examination result = await examinationService.CreateAsync(examination);
            return CreatedAtAction(nameof(GetById), new { id = result.ExaminationId }, result);
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to create examination."); return Problem(statusCode: 500, title: "Could not create examination."); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Examination examination)
    {
        try
        {
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
}
