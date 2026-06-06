using System.Net;
using System.Net.Http.Json;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class BasketsEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task GetBasket_AsClient_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.ClientEmail);

        var response = await client.GetAsync($"/api/baskets/{Ids.ClientUserId}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetBasket_AsPharmacist_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.PharmacistEmail);

        var response = await client.GetAsync($"/api/baskets/{Ids.ClientUserId}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task ClearBasket_ReturnsNoContent()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.ClientEmail);

        var response = await client.DeleteAsync($"/api/baskets/{Ids.ClientUserId}");

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    [TestMethod]
    public async Task GetBasket_AsDoctor_ReturnsForbidden()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync($"/api/baskets/{Ids.ClientUserId}");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetBasket_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync($"/api/baskets/{Ids.ClientUserId}");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
