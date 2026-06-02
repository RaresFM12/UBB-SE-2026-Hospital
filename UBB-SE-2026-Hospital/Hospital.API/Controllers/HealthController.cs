using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
        => Ok(new
        {
            status = "ok",
            solution = "UBB-SE-2026-Hospital",
            message = "Merged API skeleton is running.",
        });
}
