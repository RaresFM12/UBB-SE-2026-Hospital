#if false
using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[AuthorizeRole("Admin")]
[Route("api/users")]
public class UsersController(IUserAccountService userAccountService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<User>>> GetAll(CancellationToken cancellationToken = default)
        => Ok(await userAccountService.GetAllUsersAsync(cancellationToken));

    [HttpGet("{userId:int}")]
    public async Task<ActionResult<User>> GetById(int userId, CancellationToken cancellationToken = default)
    {
        if (!await userAccountService.UserExistsByIdAsync(userId, cancellationToken))
            return NotFound();
        return Ok(await userAccountService.GetUserByIdAsync(userId, cancellationToken));
    }

    [HttpGet("by-email")]
    public async Task<ActionResult<User>> GetByEmail([FromQuery] string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(email))
            return BadRequest("email query parameter is required");
        if (!await userAccountService.UserExistsByEmailAsync(email, cancellationToken))
            return NotFound();
        return Ok(await userAccountService.GetUserByEmailAsync(email, cancellationToken));
    }

    [HttpGet("{userId:int}/exists")]
    public async Task<ActionResult<bool>> ExistsById(int userId, CancellationToken cancellationToken = default)
        => Ok(await userAccountService.UserExistsByIdAsync(userId, cancellationToken));

    [HttpGet("exists")]
    public async Task<ActionResult<bool>> ExistsByEmail([FromQuery] string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(email))
            return BadRequest("email query parameter is required");
        return Ok(await userAccountService.UserExistsByEmailAsync(email, cancellationToken));
    }

    [HttpGet("{userId:int}/period-tracker")]
    public async Task<ActionResult<bool>> HasPeriodTracker(int userId, CancellationToken cancellationToken = default)
        => Ok(await userAccountService.UserHasPeriodTrackerAsync(userId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        await userAccountService.CreateUserAsync(
            request.Email, request.PhoneNumber, request.PasswordHash, request.Username,
            request.DiscountNotifications, request.IsDisabled, request.IsAdmin,
            request.LoyaltyPoints, request.Role, cancellationToken);
        return NoContent();
    }

    [HttpPut("{userId:int}")]
    public async Task<IActionResult> Update(int userId, [FromBody] User user, CancellationToken cancellationToken = default)
    {
        user.Id = userId;
        await userAccountService.UpdateUserAsync(user, cancellationToken);
        return NoContent();
    }

    public record CreateUserRequest(
        string Email, string PhoneNumber, string PasswordHash, string Username,
        bool DiscountNotifications, bool IsDisabled, bool IsAdmin, int LoyaltyPoints, string Role);
}
#endif
