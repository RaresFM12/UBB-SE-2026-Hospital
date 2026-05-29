using Hospital.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize]
public class PharmacyController(IAdminService adminService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
        => View(await adminService.GetItemsAsync(cancellationToken));
}
