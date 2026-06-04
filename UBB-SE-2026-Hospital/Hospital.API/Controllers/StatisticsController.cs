#if false
using Hospital.Services.PatientEr;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin")]
[Route("api/statistics")]
public class StatisticsController(IStatisticsService statisticsService, ILogger<StatisticsController> logger) : ControllerBase
{
    [HttpGet("active-vs-archived")]
    public async Task<ActionResult<Dictionary<string, int>>> GetActiveVsArchivedRatio()
    {
        try { return Ok(await statisticsService.GetActiveVsArchivedRatioAsync()); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch active/archived ratio."); return Problem(statusCode: 500, title: "Could not fetch statistics."); }
    }

    [HttpGet("age-distribution")]
    public async Task<ActionResult<Dictionary<string, int>>> GetAgeDistribution()
    {
        try { return Ok(await statisticsService.GetAgeDistributionAsync()); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch age distribution."); return Problem(statusCode: 500, title: "Could not fetch statistics."); }
    }

    [HttpGet("blood-types")]
    public async Task<ActionResult<Dictionary<string, int>>> GetPatientsByBloodType()
    {
        try { return Ok(await statisticsService.GetPatientsByBloodTypeAsync()); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch blood type distribution."); return Problem(statusCode: 500, title: "Could not fetch statistics."); }
    }

    [HttpGet("rh-factor")]
    public async Task<ActionResult<Dictionary<string, int>>> GetPatientsByRh()
    {
        try { return Ok(await statisticsService.GetPatientsByRhAsync()); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch Rh distribution."); return Problem(statusCode: 500, title: "Could not fetch statistics."); }
    }

    [HttpGet("gender-distribution")]
    public async Task<ActionResult<Dictionary<string, int>>> GetGenderDistribution()
    {
        try { return Ok(await statisticsService.GetPatientGenderDistributionAsync()); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch gender distribution."); return Problem(statusCode: 500, title: "Could not fetch statistics."); }
    }

    [HttpGet("consultations")]
    public async Task<ActionResult<Dictionary<string, int>>> GetConsultationDistribution()
    {
        try { return Ok(await statisticsService.GetConsultationDistributionAsync()); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch consultation distribution."); return Problem(statusCode: 500, title: "Could not fetch statistics."); }
    }

    [HttpGet("top-diagnoses")]
    public async Task<ActionResult<Dictionary<string, int>>> GetTopDiagnoses()
    {
        try { return Ok(await statisticsService.GetTopDiagnosesAsync()); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch top diagnoses."); return Problem(statusCode: 500, title: "Could not fetch statistics."); }
    }

    [HttpGet("top-meds")]
    public async Task<ActionResult<Dictionary<string, int>>> GetMostPrescribedMeds()
    {
        try { return Ok(await statisticsService.GetMostPrescribedMedsAsync()); }
        catch (Exception ex) { logger.LogError(ex, "Failed to fetch top meds."); return Problem(statusCode: 500, title: "Could not fetch statistics."); }
    }
}
#endif
