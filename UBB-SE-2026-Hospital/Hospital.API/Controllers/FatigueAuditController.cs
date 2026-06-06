using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin", "Manager")]
[Route("api/fatigueaudit")]
public class FatigueAuditController(IFatigueAuditService fatigueAuditService) : ControllerBase
{
    [HttpGet("run")]
    public ActionResult<AutoAuditResult> Run([FromQuery] DateTime weekStart)
        => Ok(fatigueAuditService.RunAutoAudit(weekStart));
}
