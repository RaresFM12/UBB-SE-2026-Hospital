using Hospital.Data.Models;

namespace Hospital.Shared.Proxies;

public interface IAuthenticationApiClient
{
    Task<AuthenticationResponse> LoginAsync(string username, string password, CancellationToken cancellationToken);
}


