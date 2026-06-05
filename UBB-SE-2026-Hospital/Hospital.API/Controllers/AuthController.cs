using Hospital.API.Auth;
using Hospital.Data.Models;
using Hospital.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IAuthService authService,
    IModuleAccessService moduleAccessService,
    IUserAccountService userAccountService) : ControllerBase
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

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<User>> GetCurrentUserProfile(CancellationToken cancellationToken)
    {
        UserPrincipal currentUser = this.GetCurrentUser();
        if (currentUser.Id <= 0)
        {
            return Unauthorized();
        }

        User? user = await userAccountService.GetUserByIdAsync(currentUser.Id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        UserPrincipal currentUser = this.GetCurrentUser();
        if (currentUser.Id <= 0)
        {
            return Unauthorized();
        }

        User? user = await userAccountService.GetUserByIdAsync(currentUser.Id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        user.Username = request.Username ?? string.Empty;
        user.PhoneNumber = request.PhoneNumber ?? string.Empty;
        await userAccountService.UpdateUserAsync(user, cancellationToken);
        return NoContent();
    }

    public record UpdateProfileRequest(string? Username, string? PhoneNumber);
}
