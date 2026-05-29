using Hospital.Shared.DTOs.Auth;

namespace Hospital.Shared.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
