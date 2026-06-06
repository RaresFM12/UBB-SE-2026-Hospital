using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Hospital.Data.Models;

namespace Hospital.Tests.Integration;

/// <summary>
/// Shared bootstrapping for the application-level integration tests: a single
/// in-memory API host per test class, seeded once, plus helpers for logging in
/// and creating authenticated clients.
/// </summary>
public abstract class IntegrationTestBase
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected HospitalApiFactory Factory { get; private set; } = null!;
    protected SeededIds Ids { get; private set; } = null!;

    [TestInitialize]
    public void BaseInitialize()
    {
        Factory = new HospitalApiFactory();
        Ids = Factory.SeedDatabase();
    }

    [TestCleanup]
    public void BaseCleanup() => Factory.Dispose();

    /// <summary>Anonymous client (no Authorization header).</summary>
    protected HttpClient CreateClient() => Factory.CreateClient();

    /// <summary>Logs in with the given credentials and returns the JWT.</summary>
    protected async Task<string> LoginAsync(string email, string password = SeededIds.Password)
    {
        using var client = Factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = password,
        });

        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        return auth!.Token;
    }

    /// <summary>Creates an HttpClient that authenticates as the supplied seeded user.</summary>
    protected async Task<HttpClient> CreateAuthenticatedClientAsync(string email)
    {
        var token = await LoginAsync(email);
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected Task<HttpClient> CreateAdminClientAsync() => CreateAuthenticatedClientAsync(SeededIds.AdminEmail);
}
