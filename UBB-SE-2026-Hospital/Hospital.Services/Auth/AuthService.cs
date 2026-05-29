using Hospital.Shared.DTOs.Auth;
using Hospital.Shared.Services;

namespace Hospital.Services.Auth;

public class AuthService : IAuthService
{
    public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var response = new AuthResponse
        {
            Token = $"placeholder-token-for-{request.Email}",
            RefreshToken = "placeholder-refresh-token",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(60),
        };

        return Task.FromResult(response);
    }
}
