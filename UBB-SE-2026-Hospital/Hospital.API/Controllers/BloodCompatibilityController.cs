using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[Authorize]
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
