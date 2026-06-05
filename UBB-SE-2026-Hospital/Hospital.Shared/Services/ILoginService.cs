namespace Hospital.Shared.Services;

public interface ILoginService
{
    Task<bool> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<bool> RegisterAsync(string email, string password, string phoneNumber, string username, CancellationToken cancellationToken = default);
}
