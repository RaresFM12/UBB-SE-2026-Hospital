using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize]
public class PharmacyController : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return View(Array.Empty<Hospital.Shared.Models.StaffPharmacy.Item>());
    }
}
