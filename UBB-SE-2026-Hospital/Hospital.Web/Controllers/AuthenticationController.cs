using Hospital.Shared.Proxies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

[AllowAnonymous]
public class AuthenticationController : Controller
{
    [HttpGet]
    public IActionResult AuthenticationView()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return RedirectToAction("Login", "Auth");
    }

    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction(nameof(AuthenticationView));
    }
}
