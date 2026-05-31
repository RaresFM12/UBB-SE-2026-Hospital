using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Shared.DTOs.Auth;
using Hospital.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Hospital.Services.Auth;

public class AuthService(IUsersRepository usersRepository, IConfiguration configuration) : IAuthService
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        User user = await usersRepository.GetUserByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (user.IsDisabled || !VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        int expiryMinutes = int.TryParse(configuration["Jwt:ExpiryMinutes"], out int minutes) ? minutes : 60;
        DateTime expiresAtUtc = DateTime.UtcNow.AddMinutes(expiryMinutes);

        return new AuthResponse
        {
            Token = GenerateToken(user, expiresAtUtc),
            RefreshToken = Guid.NewGuid().ToString("N"),
            ExpiresAtUtc = expiresAtUtc,
        };
    }

    private string GenerateToken(User user, DateTime expiresAtUtc)
    {
        string signingKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT signing key (Jwt:Key) is not configured.");
        string? issuer = configuration["Jwt:Issuer"];
        string? audience = configuration["Jwt:Audience"];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        if (TryVerifyPbkdf2(password, storedHash, out bool isMatch))
            return isMatch;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(password),
            Encoding.UTF8.GetBytes(storedHash));
    }

    private static bool TryVerifyPbkdf2(string password, string storedHash, out bool isMatch)
    {
        isMatch = false;
        try
        {
            byte[] combined = Convert.FromBase64String(storedHash);
            if (combined.Length != 48)
                return false;

            byte[] salt = combined[..16];
            byte[] expectedHash = combined[16..];
            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password), salt, 100_000, HashAlgorithmName.SHA256, 32);

            isMatch = CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
