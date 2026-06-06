using System.Net;
using System.Net.Http.Json;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class HighRiskMedicinesEndpointsTests : IntegrationTestBase
{
    private sealed record HighRiskMedicineSummary(string MedicineName, string WarningMessage);

    [TestMethod]
    public async Task GetAll_AsPharmacist_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.PharmacistEmail);

        var response = await client.GetAsync("/api/high-risk-medicines");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<HighRiskMedicineSummary>>(JsonOptions);
        Assert.IsNotNull(items);
    }

    [TestMethod]
    public async Task GetAll_AsAdmin_ReturnsOk()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/high-risk-medicines");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_AsDoctor_ReturnsForbidden()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync("/api/high-risk-medicines");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/high-risk-medicines");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
