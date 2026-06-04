using Hospital.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hospital.Shared.Services;

namespace Hospital.Web.Controllers;

[Authorize]
public class PharmacyController(IAdminService adminService) : Controller
{
    public IActionResult Index()
    {
        // TODO: inject and wire up admin service
        return View();
    }
}
