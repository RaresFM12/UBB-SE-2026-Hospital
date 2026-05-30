namespace Hospital.Data.Models.DTOs;

public record LoginDto
{
    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}

public record RegisterDto
{
    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string Role { get; init; } = "User";
}

public record AuthResponseDto
{
    public string Token { get; init; } = string.Empty;

    public string Username { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;
}
