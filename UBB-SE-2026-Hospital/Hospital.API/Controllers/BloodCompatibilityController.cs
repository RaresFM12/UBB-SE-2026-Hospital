using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Services.PatientEr;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Doctor","Nurse")]
[Route("api/bloodcompatibilities")]
public class BloodCompatibilityController(
    IBloodCompatibilityService bloodCompatibilityService,
    ILogger<BloodCompatibilityController> logger) : ControllerBase
{
    [HttpPost("top-donors")]
    public async Task<ActionResult<List<Patient>>> GetTopCompatibleDonors([FromBody] GetTopCompatibleDonorsRequest dto)
    {
        try
        {
            return Ok(await bloodCompatibilityService.GetTopCompatibleDonorsAsync(dto.RecipientId));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to compute compatible donors.");
            return Problem(detail: "Failed to compute compatible donors.", statusCode: 500, title: "Blood compatibility error.");
        }
    }
}
