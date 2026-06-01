using System.Security.Claims;
using Hospital.Shared.Services;
using Hospital.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService authService;

    public AuthController(IAuthService authService)
    {
        this.authService = authService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["HideShell"] = true;
        ViewData["ReturnUrl"] = returnUrl;
        return View("AuthenticationView", new AuthenticationViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Login(AuthenticationViewModel model, string? returnUrl, CancellationToken cancellationToken)
    {
        ViewData["HideShell"] = true;

        if (!ModelState.IsValid)
        {
            return View("AuthenticationView", model);
        }

        try
        {
            var response = await authService.LoginAsync(model.Username.Trim(), model.Password, cancellationToken);

            HttpContext.Session.SetString("AccessToken", response.Token);
            HttpContext.Session.SetString("Username", response.Username);
            HttpContext.Session.SetString("Role", response.Role);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, response.Username),
                new(ClaimTypes.Role, response.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
        catch (UnauthorizedAccessException e)
        {
            model.ErrorMessage = e.Message;
            ModelState.AddModelError(string.Empty, e.Message);
            return View("AuthenticationView", model);
        }
        catch (Exception e)
        {
            model.ErrorMessage = e.Message;
            ModelState.AddModelError(string.Empty, e.Message);
            return View("AuthenticationView", model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Remove("AccessToken");
        HttpContext.Session.Remove("Username");
        HttpContext.Session.Remove("Role");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}