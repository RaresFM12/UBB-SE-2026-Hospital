using Hospital.Data.Models;
using Hospital.Services.PatientEr;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Doctor","Nurse")]
[Route("api/allergies")]
public class AllergyController(IAllergyService allergyService, ILogger<AllergyController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Allergy>>> GetAll()
    {
        try
        {
            return Ok(await allergyService.GetAllergiesAsync());
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch allergies.");
            return Problem(detail: "Failed to fetch allergies.", statusCode: 500, title: "Could not fetch allergies.");
        }
    }
}
