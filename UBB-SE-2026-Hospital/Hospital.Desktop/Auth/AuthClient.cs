using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Hospital.Shared.DTOs.Auth;

namespace Hospital.Desktop.Auth;

public class AuthClient(HttpClient httpClient)
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
    }

    public void Logout()
    {
        TokenStorage.CurrentToken = null;
        httpClient.DefaultRequestHeaders.Authorization = null;
    }
}
