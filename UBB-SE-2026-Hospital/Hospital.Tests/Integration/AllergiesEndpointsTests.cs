using System.Net;
using System.Net.Http.Json;
using Hospital.Data.Models;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class AllergiesEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task GetAll_AsDoctor_ReturnsSeededAllergies()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var allergies = await client.GetFromJsonAsync<List<Allergy>>("/api/allergies", JsonOptions);

        Assert.IsNotNull(allergies);
        // 10 allergies are seeded via HasData.
        Assert.IsTrue(allergies!.Count >= 10);
        Assert.IsTrue(allergies.Any(a => a.AllergyName == "Penicillin"));
    }

    [TestMethod]
    public async Task GetAll_AsNurse_IsAllowed()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.NurseEmail);

        var response = await client.GetAsync("/api/allergies");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_AsPharmacist_ReturnsForbidden()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.PharmacistEmail);

        var response = await client.GetAsync("/api/allergies");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/allergies");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
