using Hospital.Shared.Proxies;
using Hospital.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize]
public class PharmacyController(IAdminApiClient adminService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
        => View(await adminService.GetItemsAsync(null, cancellationToken));
}

