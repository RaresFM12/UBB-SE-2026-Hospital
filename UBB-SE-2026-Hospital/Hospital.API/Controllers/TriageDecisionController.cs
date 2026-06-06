using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin", "Doctor", "Nurse", "ERDoctor")]
[Route("api/triage-decision")]
public class TriageDecisionController(ITriageDecisionService triageDecisionService) : ControllerBase
{
    [HttpPost("level")]
    public ActionResult<int> CalculateLevel([FromBody] TriageParameters parameters)
        => Ok(triageDecisionService.CalculateTriageLevel(parameters));

    [HttpPost("specialization")]
    public ActionResult<string> DetermineSpecialization([FromBody] TriageParameters parameters)
        => Ok(triageDecisionService.DetermineSpecialization(parameters));
}
