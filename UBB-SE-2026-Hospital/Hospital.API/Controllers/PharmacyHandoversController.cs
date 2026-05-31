using Hospital.Data.Models;
using Hospital.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[Authorize]
[Route("api/pharmacy-handovers")]
public class PharmacyHandoversController(IPharmacyHandoverService pharmacyHandoverService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PharmacyHandover>>> GetAll(CancellationToken cancellationToken = default)
        => Ok(await pharmacyHandoverService.GetAllPharmacyHandoversAsync(cancellationToken));
}
