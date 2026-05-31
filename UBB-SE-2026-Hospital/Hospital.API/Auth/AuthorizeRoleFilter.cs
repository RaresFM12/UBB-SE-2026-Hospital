using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Hospital.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Hospital.API.Auth;

public sealed class AuthorizeRoleFilter : IAsyncAuthorizationFilter
{
    private readonly string[] allowedRoles;
    private readonly IUsersRepository usersRepository;

    public AuthorizeRoleFilter(string[] roles, IUsersRepository usersRepository)
    {
        allowedRoles = roles;
        this.usersRepository = usersRepository;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        ClaimsPrincipal principal = context.HttpContext.User;

        if (principal.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        string? rawId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(rawId, out int userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var user = await usersRepository.GetUserByIdAsync(userId);
        if (user is null || user.IsDisabled)
        {
            context.Result = new ForbidResult();
            return;
        }

        if (allowedRoles.Length > 0 &&
            !allowedRoles.Contains(user.Role, StringComparer.OrdinalIgnoreCase))
        {
            context.Result = new ForbidResult();
        }
    }
}
