using Hospital.Shared.Models.StaffPharmacy;
using Hospital.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[Authorize]
[Route("api/items")]
public class ItemsController(IAdminService adminService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Item>>> GetAll(CancellationToken cancellationToken)
        => Ok(await adminService.GetItemsAsync(cancellationToken));
}
