using Hospital.Data.Models;
using Hospital.Data.Models;
using Hospital.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Doctor","Nurse")]
[Route("api/addicts")]
public class AddictDetectionController(IAddictDetectionService addictDetectionService) : ControllerBase
{
    [HttpGet("candidates")]
    public async Task<ActionResult<List<Patient>>> GetAddictCandidates()
    {
        try { return Ok(await addictDetectionService.GetAddictCandidatesAsync()); }
        catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500, title: "Could not get addict candidates."); }
    }

    [HttpPost("police-report")]
    public async Task<ActionResult<string>> BuildPoliceReport([FromBody] BuildPoliceReportRequest dto)
    {
        try { return Ok(await addictDetectionService.BuildPoliceReportAsync(dto.PatientId)); }
        catch (ArgumentException ex) { return Problem(detail: ex.Message, statusCode: 400, title: "Invalid patient data."); }
        catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500, title: "Could not build police report."); }
    }

    [HttpPost("{patientId:int}/notify")]
    public async Task<ActionResult> MarkPoliceNotified([FromRoute] int patientId)
    {
        try { await addictDetectionService.MarkPoliceNotifiedAsync(patientId); return Ok(); }
        catch (ArgumentException ex) { return Problem(detail: ex.Message, statusCode: 400, title: "Invalid patient ID."); }
        catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500, title: "Could not mark patient as notified."); }
    }

    [HttpGet("{patientId:int}/chronic-conditions")]
    public async Task<ActionResult<string>> GetChronicConditions([FromRoute] int patientId)
    {
        try { return Ok(await addictDetectionService.GetChronicConditionsAsync(patientId)); }
        catch (ArgumentException ex) { return Problem(detail: ex.Message, statusCode: 400, title: "Invalid patient ID."); }
        catch (Exception ex) { return Problem(detail: ex.Message, statusCode: 500, title: "Could not get chronic conditions."); }
    }
}
