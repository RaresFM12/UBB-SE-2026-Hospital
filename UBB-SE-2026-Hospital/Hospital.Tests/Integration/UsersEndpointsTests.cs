using System.Net;
using System.Net.Http.Json;
using Hospital.Data.Models;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class UsersEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task GetAll_AsAdmin_ReturnsSeededUsers()
    {
        using var client = await CreateAdminClientAsync();

        var users = await client.GetFromJsonAsync<List<User>>("/api/users", JsonOptions);

        Assert.IsNotNull(users);
        Assert.IsTrue(users!.Count >= 6);
        Assert.IsTrue(users.Any(u => u.Email == SeededIds.AdminEmail));
    }

    [TestMethod]
    public async Task GetAll_AsDoctor_ReturnsForbidden()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync("/api/users");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetById_ExistingUser_ReturnsUser()
    {
        using var client = await CreateAdminClientAsync();

        var user = await client.GetFromJsonAsync<User>($"/api/users/{Ids.ClientUserId}", JsonOptions);

        Assert.IsNotNull(user);
        Assert.AreEqual(SeededIds.ClientEmail, user!.Email);
    }

    [TestMethod]
    public async Task GetById_MissingUser_ReturnsNotFound()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/users/999999");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task GetByEmail_WithoutQuery_ReturnsBadRequest()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/users/by-email?email=");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task GetByEmail_ExistingUser_ReturnsUser()
    {
        using var client = await CreateAdminClientAsync();

        var user = await client.GetFromJsonAsync<User>(
            $"/api/users/by-email?email={Uri.EscapeDataString(SeededIds.NurseEmail)}", JsonOptions);

        Assert.IsNotNull(user);
        Assert.AreEqual(SeededIds.NurseEmail, user!.Email);
    }

    [TestMethod]
    public async Task ExistsById_ReturnsTrueForSeededUser()
    {
        using var client = await CreateAdminClientAsync();

        var exists = await client.GetFromJsonAsync<bool>($"/api/users/{Ids.AdminUserId}/exists", JsonOptions);

        Assert.IsTrue(exists);
    }

    [TestMethod]
    public async Task ExistsById_ReturnsFalseForUnknownUser()
    {
        using var client = await CreateAdminClientAsync();

        var exists = await client.GetFromJsonAsync<bool>("/api/users/424242/exists", JsonOptions);

        Assert.IsFalse(exists);
    }

    [TestMethod]
    public async Task ExistsByEmail_MissingQuery_ReturnsBadRequest()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/users/exists?email=");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Create_NewUser_PersistsAndIsRetrievable()
    {
        using var client = await CreateAdminClientAsync();
        var request = new
        {
            Email = "new.user@hospital.test",
            PhoneNumber = "0722222222",
            PasswordHash = "hash",
            Username = "newuser",
            DiscountNotifications = false,
            IsDisabled = false,
            IsAdmin = false,
            LoyaltyPoints = 0,
            Role = "Client",
        };

        var createResponse = await client.PostAsJsonAsync("/api/users", request);

        Assert.AreEqual(HttpStatusCode.NoContent, createResponse.StatusCode);
        var exists = await client.GetFromJsonAsync<bool>(
            $"/api/users/exists?email={Uri.EscapeDataString("new.user@hospital.test")}", JsonOptions);
        Assert.IsTrue(exists);
    }

    [TestMethod]
    public async Task Update_ChangesUsername()
    {
        using var client = await CreateAdminClientAsync();
        var user = await client.GetFromJsonAsync<User>($"/api/users/{Ids.ClientUserId}", JsonOptions);
        user!.Username = "renamed-client";

        var response = await client.PutAsJsonAsync($"/api/users/{Ids.ClientUserId}", user);

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        var updated = await client.GetFromJsonAsync<User>($"/api/users/{Ids.ClientUserId}", JsonOptions);
        Assert.AreEqual("renamed-client", updated!.Username);
    }

    [TestMethod]
    public async Task HasPeriodTracker_ForSeededUser_ReturnsTrue()
    {
        using var client = await CreateAdminClientAsync();

        var result = await client.GetFromJsonAsync<bool>(
            $"/api/users/{Ids.ClientUserId}/period-tracker", JsonOptions);

        Assert.IsTrue(result);
    }
}
