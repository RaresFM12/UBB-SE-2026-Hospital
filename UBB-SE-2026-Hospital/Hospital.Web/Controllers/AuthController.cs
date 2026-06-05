using Hospital.Shared.Proxies;
using System.Security.Claims;
using Hospital.Shared.Services;
using Hospital.Data.Models; 
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using LoginRequest = Hospital.Data.Models.LoginRequest;

namespace Hospital.Web.Controllers;

public class AuthController : Controller
{
    private readonly IAuthenticationApiClient _authService;

    public AuthController(IAuthenticationApiClient authService)
    {
        _authService = authService;
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

        return View(new LoginRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest model, string? returnUrl, CancellationToken cancellationToken)
    {
        ViewData["HideShell"] = true;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var response = await _authService.LoginAsync(model.Email, model.Password, cancellationToken);

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(response.Token);

            var userId = jwtToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub)?.Value;
            var username = jwtToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.UniqueName)?.Value ?? model.Email;
            var role = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role)?.Value ?? "Client";
            var email = jwtToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value ?? model.Email;

            HttpContext.Session.SetString("AccessToken", response.Token);
            HttpContext.Session.SetString("Username", username);
            HttpContext.Session.SetString("Role", role);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, username),
                new(ClaimTypes.Role, role),
                new(ClaimTypes.Email, email)
            };

            if (!string.IsNullOrWhiteSpace(userId))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            }

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
            ModelState.AddModelError(string.Empty, e.Message);
            return View(model);
        }
        catch (Exception e)
        {
            ModelState.AddModelError(string.Empty, e.Message);
            return View(model);
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
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

