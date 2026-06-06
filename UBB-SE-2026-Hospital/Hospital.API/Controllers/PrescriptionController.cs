using Hospital.Data.Models;
using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Doctor","Nurse")]
[Route("api/prescriptions")]
public class PrescriptionController(IPrescriptionService prescriptionService, ILogger<PrescriptionController> logger) : ControllerBase
{
    [HttpGet("latest")]
    public async Task<ActionResult<List<Prescription>>> GetLatest([FromQuery] LatestPrescriptionsQuery dto)
    {
        try
        {
            return Ok(await prescriptionService.GetLatestPrescriptionsAsync(dto.N, dto.Page));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch latest prescriptions.");
            return Problem(detail: "Failed to fetch latest prescriptions.", statusCode: 500, title: "Could not fetch prescriptions.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<List<Prescription>>> GetPrescriptions([FromBody] PrescriptionFilter? filter)
    {
        try
        {
            return Ok(await prescriptionService.ApplyFilterAsync(filter ?? new PrescriptionFilter()));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch prescriptions.");
            return Problem(detail: "Failed to fetch prescriptions.", statusCode: 500, title: "Could not fetch prescriptions.");
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Prescription>> GetById(int id)
    {
        try
        {
            return Ok(await prescriptionService.GetPrescriptionDetailsAsync(id));
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Prescription {Id} not found.", id);
            return Problem(detail: ex.Message, statusCode: 404, title: "Prescription not found.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch prescription {Id}.", id);
            return Problem(detail: "Failed to fetch prescription details.", statusCode: 500, title: "Could not fetch prescription.");
        }
    }
}
