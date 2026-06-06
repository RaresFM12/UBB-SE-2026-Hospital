using Hospital.Data.Models;
using Hospital.Shared.Services;
using Hospital.API.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserAccountService userAccountService) : ControllerBase
{
    [AuthorizeRole("Admin")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<User>>> GetAll(CancellationToken cancellationToken = default)
        => Ok(await userAccountService.GetAllUsersAsync(cancellationToken));

    [AuthorizeRole()]
    [HttpGet("{userId:int}")]
    public async Task<ActionResult<User>> GetById(int userId, CancellationToken cancellationToken = default)
    {
        if (!CanAccessUser(userId))
        {
            return Forbid();
        }

        if (!await userAccountService.UserExistsByIdAsync(userId, cancellationToken))
            return NotFound();
        return Ok(await userAccountService.GetUserByIdAsync(userId, cancellationToken));
    }

    [AuthorizeRole("Admin")]
    [HttpGet("by-email")]
    public async Task<ActionResult<User>> GetByEmail([FromQuery] string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(email))
            return BadRequest("email query parameter is required");
        if (!await userAccountService.UserExistsByEmailAsync(email, cancellationToken))
            return NotFound();
        return Ok(await userAccountService.GetUserByEmailAsync(email, cancellationToken));
    }

    [AuthorizeRole()]
    [HttpGet("{userId:int}/exists")]
    public async Task<ActionResult<bool>> ExistsById(int userId, CancellationToken cancellationToken = default)
    {
        if (!CanAccessUser(userId))
        {
            return Forbid();
        }

        return Ok(await userAccountService.UserExistsByIdAsync(userId, cancellationToken));
    }

    [AuthorizeRole("Admin")]
    [HttpGet("exists")]
    public async Task<ActionResult<bool>> ExistsByEmail([FromQuery] string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(email))
            return BadRequest("email query parameter is required");
        return Ok(await userAccountService.UserExistsByEmailAsync(email, cancellationToken));
    }

    [AuthorizeRole("Admin")]
    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<User>>> Search([FromQuery] string q, CancellationToken cancellationToken = default)
    {
        var results = await userAccountService.SearchUsersAsync(q, cancellationToken);
        return Ok(results);
    }

    [AuthorizeRole("Admin")]
    [HttpPost("{userId:int}/promote")]
    public async Task<IActionResult> Promote(int userId, CancellationToken cancellationToken = default)
    {
        await userAccountService.PromoteToAdminAsync(userId, cancellationToken);
        return NoContent();
    }

    [AuthorizeRole("Admin")]
    [HttpPost("{userId:int}/disable")]
    public async Task<IActionResult> Disable(int userId, CancellationToken cancellationToken = default)
    {
        await userAccountService.DisableAccountAsync(userId, cancellationToken);
        return NoContent();
    }

    [AuthorizeRole()]
    [HttpGet("{userId:int}/period-tracker")]
    public async Task<ActionResult<bool>> HasPeriodTracker(int userId, CancellationToken cancellationToken = default)
    {
        if (!CanAccessUser(userId))
        {
            return Forbid();
        }

        return Ok(await userAccountService.UserHasPeriodTrackerAsync(userId, cancellationToken));
    }

    [AuthorizeRole()]
    [HttpGet("{userId:int}/has-period-tracker")]
    public Task<ActionResult<bool>> HasPeriodTrackerAlias(int userId, CancellationToken cancellationToken = default)
        => HasPeriodTracker(userId, cancellationToken);

    [AuthorizeRole("Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        await userAccountService.CreateUserAsync(
            request.Email, request.PhoneNumber, request.PasswordHash, request.Username,
            request.DiscountNotifications, request.IsDisabled, request.IsAdmin,
            request.LoyaltyPoints, request.Role, cancellationToken);
        return NoContent();
    }

    [AuthorizeRole()]
    [HttpPut("{userId:int}")]
    public async Task<IActionResult> Update(int userId, [FromBody] User user, CancellationToken cancellationToken = default)
    {
        if (!CanAccessUser(userId))
        {
            return Forbid();
        }

        if (!IsAdmin())
        {
            User? existingUser = await userAccountService.GetUserByIdAsync(userId, cancellationToken);
            if (existingUser is null)
            {
                return NotFound();
            }

            existingUser.Username = user.Username;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.DiscountNotifications = user.DiscountNotifications;
            existingUser.StartPeriodDate = user.StartPeriodDate;
            existingUser.CycleDays = user.CycleDays;
            existingUser.PeriodLasts = user.PeriodLasts;
            existingUser.PremenstrualSyndromeOption = user.PremenstrualSyndromeOption;
            await userAccountService.UpdateUserAsync(existingUser, cancellationToken);
            return NoContent();
        }

        user.Id = userId;
        await userAccountService.UpdateUserAsync(user, cancellationToken);
        return NoContent();
    }

    public record CreateUserRequest(
        string Email, string PhoneNumber, string PasswordHash, string Username,
        bool DiscountNotifications, bool IsDisabled, bool IsAdmin, int LoyaltyPoints, string Role);

    private bool CanAccessUser(int userId)
        => this.GetCurrentUser().Id == userId || IsAdmin();

    private bool IsAdmin()
        => string.Equals(this.GetCurrentUser().Role, "Admin", StringComparison.OrdinalIgnoreCase);
}
