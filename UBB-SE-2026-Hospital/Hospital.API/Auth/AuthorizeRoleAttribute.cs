using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Auth;

// DB-backed role authorization. The allowed role names are checked against the
// caller's CURRENT role loaded from the database (see AuthorizeRoleFilter),
// so authorization does not blindly trust the role string carried in the token.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class AuthorizeRoleAttribute : TypeFilterAttribute
{
    public AuthorizeRoleAttribute(params string[] roles)
        : base(typeof(AuthorizeRoleFilter))
    {
        Arguments = [roles];
    }
}
