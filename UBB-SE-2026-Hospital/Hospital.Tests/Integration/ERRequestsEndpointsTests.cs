using System.Net;
using System.Net.Http.Json;
using Hospital.Data.Models;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class ERRequestsEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task GetAll_AsDoctor_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync("/api/er-requests");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<ERRequest>>(JsonOptions);
        Assert.IsNotNull(items);
    }

    [TestMethod]
    public async Task GetPending_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.NurseEmail);

        var response = await client.GetAsync("/api/er-requests/pending");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetById_Missing_ReturnsNotFound()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync("/api/er-requests/999999");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Create_Then_GetById_ReturnsRequest()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);
        var request = new { Specialization = "Cardiology", Location = "Zone A", Status = "PENDING" };

        var createResponse = await client.PostAsJsonAsync("/api/er-requests", request);

        Assert.AreEqual(HttpStatusCode.OK, createResponse.StatusCode);
        var id = await createResponse.Content.ReadFromJsonAsync<int>(JsonOptions);
        Assert.IsGreaterThan(0, id);

        var getResponse = await client.GetAsync($"/api/er-requests/{id}");
        Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [TestMethod]
    public async Task Simulate_ReturnsCreatedIds()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);
        var request = new { Count = 3 };

        var response = await client.PostAsJsonAsync("/api/er-requests/simulate", request);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var ids = await response.Content.ReadFromJsonAsync<List<int>>(JsonOptions);
        Assert.IsNotNull(ids);
    }

    [TestMethod]
    public async Task GetAll_AsPharmacist_ReturnsForbidden()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.PharmacistEmail);

        var response = await client.GetAsync("/api/er-requests");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/er-requests");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
