using Hospital.API.Auth;
using Hospital.Shared.DTOs.Auth;
using Hospital.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService, IModuleAccessService moduleAccessService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await authService.LoginAsync(request, cancellationToken));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [Authorize]
    [HttpGet("modules")]
    public async Task<ActionResult<IReadOnlyList<ModuleDto>>> GetAccessibleModules(CancellationToken cancellationToken)
        => Ok(await moduleAccessService.GetAccessibleModulesAsync(this.GetCurrentUser().Id, cancellationToken));

    [Authorize]
    [HttpGet("modules/{moduleKey}/access")]
    public async Task<ActionResult<bool>> CanAccessModule(string moduleKey, CancellationToken cancellationToken)
        => Ok(await moduleAccessService.CanAccessModuleAsync(this.GetCurrentUser().Id, moduleKey, cancellationToken));
}
