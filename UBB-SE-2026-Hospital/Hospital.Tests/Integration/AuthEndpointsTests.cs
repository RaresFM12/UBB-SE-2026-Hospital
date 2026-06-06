using System.Net;
using System.Net.Http.Json;
using Hospital.Data.Models;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class AuthEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = SeededIds.AdminEmail,
            Password = SeededIds.Password,
        });

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.IsNotNull(auth);
        Assert.IsFalse(string.IsNullOrWhiteSpace(auth!.Token));
        Assert.IsTrue(auth.ExpiresAtUtc > DateTime.UtcNow);
    }

    [TestMethod]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = SeededIds.AdminEmail,
            Password = "not-the-password",
        });

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "ghost@hospital.test",
            Password = SeededIds.Password,
        });

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task Login_WithDisabledAccount_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = SeededIds.DisabledEmail,
            Password = SeededIds.Password,
        });

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetModules_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/auth/modules");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetModules_AsAdmin_ReturnsAllSixteenModules()
    {
        using var client = await CreateAdminClientAsync();

        var modules = await client.GetFromJsonAsync<List<ModuleDto>>("/api/auth/modules", JsonOptions);

        Assert.IsNotNull(modules);
        Assert.HasCount(16, modules!);
    }

    [TestMethod]
    public async Task GetModules_AsPharmacist_ReturnsOnlyPharmacistModules()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.PharmacistEmail);

        var modules = await client.GetFromJsonAsync<List<ModuleDto>>("/api/auth/modules", JsonOptions);

        Assert.IsNotNull(modules);
        CollectionAssert.AreEquivalent(
            new[] { "pharmacy", "orders", "prescriptions", "billing" },
            modules!.Select(m => m.Key).ToArray());
    }

    [TestMethod]
    public async Task CanAccessModule_ReflectsRolePermissions()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.PharmacistEmail);

        var allowed = await client.GetFromJsonAsync<bool>("/api/auth/modules/pharmacy/access", JsonOptions);
        var denied = await client.GetFromJsonAsync<bool>("/api/auth/modules/statistics/access", JsonOptions);

        Assert.IsTrue(allowed);
        Assert.IsFalse(denied);
    }
}
