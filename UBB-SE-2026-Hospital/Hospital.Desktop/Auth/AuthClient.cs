using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hospital.Shared.DTOs.Auth;
using Hospital.Shared.Models;
using Hospital.Shared.Services;

namespace Hospital.Desktop.Auth;

public class AuthClient(HttpClient httpClient, ICurrentUserService currentUserService)
{
    public async Task LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var request = new LoginRequest { Email = email, Password = password };
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/auth/login", request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Login failed: {(int)response.StatusCode} {response.ReasonPhrase}");

        AuthResponse auth = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken)
            ?? throw new InvalidOperationException("The authentication response was empty.");

        TokenStorage.CurrentToken = auth.Token;
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        // Populate CurrentUserService with logged-in user properties from the JWT token
        SetUserFromToken(auth.Token, currentUserService);
    }

    public void Logout()
    {
        TokenStorage.CurrentToken = null;
        httpClient.DefaultRequestHeaders.Authorization = null;
        currentUserService.SetFromUser(new User { Id = 0, Role = "Client" });
    }

    private static void SetUserFromToken(string jwtToken, ICurrentUserService currentService)
    {
        try
        {
            string[] tokenParts = jwtToken.Split('.');
            if (tokenParts.Length > 1)
            {
                string encodedPayload = tokenParts[1];
                int paddingNeeded = encodedPayload.Length % 4;
                if (paddingNeeded > 0)
                {
                    encodedPayload += new string('=', 4 - paddingNeeded);
                }
                encodedPayload = encodedPayload.Replace('-', '+').Replace('_', '/');
                byte[] decodedBytes = Convert.FromBase64String(encodedPayload);
                string jsonPayload = System.Text.Encoding.UTF8.GetString(decodedBytes);

                using var jsonDocument = JsonDocument.Parse(jsonPayload);
                JsonElement rootElement = jsonDocument.RootElement;

                int tokenUserId = 0;
                if (rootElement.TryGetProperty("sub", out var subProperty) && 
                    int.TryParse(subProperty.GetString(), out int parsedId))
                {
                    tokenUserId = parsedId;
                }

                string tokenUserEmail = "";
                if (rootElement.TryGetProperty("email", out var emailProperty))
                {
                    tokenUserEmail = emailProperty.GetString() ?? "";
                }

                string tokenUsername = "";
                if (rootElement.TryGetProperty("unique_name", out var nameProperty))
                {
                    tokenUsername = nameProperty.GetString() ?? "";
                }

                string tokenUserRole = "";
                if (rootElement.TryGetProperty("role", out var roleProperty1))
                {
                    tokenUserRole = roleProperty1.GetString() ?? "";
                }
                else if (rootElement.TryGetProperty("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out var roleProperty2))
                {
                    tokenUserRole = roleProperty2.GetString() ?? "";
                }

                var authenticatedUser = new User
                {
                    Id = tokenUserId,
                    Email = tokenUserEmail,
                    Username = tokenUsername,
                    Role = tokenUserRole,
                    IsAdmin = string.Equals(tokenUserRole, "Admin", StringComparison.OrdinalIgnoreCase)
                };

                currentService.SetFromUser(authenticatedUser);
            }
        }
        catch
        {
            // Fallback silently if JWT parsing fails
        }
    }
}
