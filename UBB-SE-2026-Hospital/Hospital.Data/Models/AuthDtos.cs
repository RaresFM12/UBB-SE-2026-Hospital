namespace Hospital.Data.Models;

public record RegisterRequest
{
    private const string DefaultRole = "User";

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string Role { get; init; } = DefaultRole;
}

public record AuthenticationResponse
{
    public string Token { get; init; } = string.Empty;

    public string Username { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;
}
