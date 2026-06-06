using System.Net;
using System.Net.Http.Json;
using Hospital.Data.Models;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class PharmacyHandoversEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task GetAll_AsPharmacist_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.PharmacistEmail);

        var response = await client.GetAsync("/api/pharmacy-handovers");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<PharmacyHandover>>(JsonOptions);
        Assert.IsNotNull(items);
    }

    [TestMethod]
    public async Task GetAll_AsAdmin_ReturnsOk()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/pharmacy-handovers");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_AsNurse_ReturnsForbidden()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.NurseEmail);

        var response = await client.GetAsync("/api/pharmacy-handovers");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/pharmacy-handovers");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
