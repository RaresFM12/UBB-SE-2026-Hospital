using System.Net;
using System.Net.Http.Json;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class BillingEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task ApplyDiscount_AsPharmacist_ReturnsComputedPrice()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.PharmacistEmail);
        var request = new { BasePrice = 200m, Discount = 10 };

        var response = await client.PostAsJsonAsync("/api/billing/discount", request);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var finalPrice = await response.Content.ReadFromJsonAsync<decimal>(JsonOptions);
        Assert.AreEqual(180m, finalPrice);
    }

    [TestMethod]
    public async Task ApplyDiscount_AsAdmin_ReturnsOk()
    {
        using var client = await CreateAdminClientAsync();
        var request = new { BasePrice = 100m, Discount = 0 };

        var response = await client.PostAsJsonAsync("/api/billing/discount", request);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var finalPrice = await response.Content.ReadFromJsonAsync<decimal>(JsonOptions);
        Assert.AreEqual(100m, finalPrice);
    }

    [TestMethod]
    public async Task ComputeBasePrice_WithUnknownRecord_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.PharmacistEmail);

        var response = await client.GetAsync($"/api/billing/base-price/{Ids.ActivePatientId}/999999");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var basePrice = await response.Content.ReadFromJsonAsync<decimal>(JsonOptions);
        Assert.AreEqual(0m, basePrice);
    }

    [TestMethod]
    public async Task PersistDiscount_WithUnknownRecord_ReturnsNotFound()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.PharmacistEmail);
        var request = new { BasePrice = 200m, Discount = 10 };

        var response = await client.PostAsJsonAsync("/api/billing/discount/999999", request);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task ApplyDiscount_AsDoctor_ReturnsForbidden()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);
        var request = new { BasePrice = 200m, Discount = 10 };

        var response = await client.PostAsJsonAsync("/api/billing/discount", request);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task ApplyDiscount_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();
        var request = new { BasePrice = 200m, Discount = 10 };

        var response = await client.PostAsJsonAsync("/api/billing/discount", request);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
