using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin","Pharmacist")]
[Route("api/high-risk-medicines")]
public class HighRiskMedicinesController(IAdminService adminService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HighRiskMedicineSummary>>> GetAll(CancellationToken cancellationToken = default)
    {
        var medicines = await adminService.GetHighRiskMedicinesAsync(cancellationToken);
        var summaries = medicines
            .Select(m => new HighRiskMedicineSummary(m.MedicineName, m.WarningMessage))
            .ToList();
        return Ok(summaries);
    }

    public record HighRiskMedicineSummary(string MedicineName, string WarningMessage);
}
