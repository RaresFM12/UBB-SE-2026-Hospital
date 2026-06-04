using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hospital.Shared.Services;

namespace Hospital.Web.Controllers;

[Authorize]
public class PharmacyController(IAdminService adminService) : Controller
{
    public IActionResult Index()
    {
        // Use synchronous call to the shared service contract available in this solution.
        var items = adminService.GetAllItems();
        return View(items);
    }
}
