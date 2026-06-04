using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[Authorize]
public class PharmacyController : Controller
{
    public IActionResult Index()
    {
        // TODO: inject and wire up admin service
        return View();
    }
}
