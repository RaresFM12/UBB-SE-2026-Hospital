using Hospital.Data.Models;

namespace Hospital.Web.Services;

public interface IAuthenticationApiClient
{
    Task<AuthenticationResponse> LoginAsync(string username, string password, CancellationToken cancellationToken);
}
