using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace Hospital.Web.Services;

public class AuthTokenForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public AuthTokenForwardingHandler(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpContext? context = httpContextAccessor.HttpContext;
        string? token = context?.Session.GetString(WebSessionKeys.AccessToken);
        if (string.IsNullOrWhiteSpace(token) && context is not null)
        {
            token = await context.GetTokenAsync("access_token");
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
