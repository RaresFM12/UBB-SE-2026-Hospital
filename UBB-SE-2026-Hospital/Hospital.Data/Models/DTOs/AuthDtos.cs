namespace Hospital.Data.Models.DTOs;

public record LoginRequest
{
    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}

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
