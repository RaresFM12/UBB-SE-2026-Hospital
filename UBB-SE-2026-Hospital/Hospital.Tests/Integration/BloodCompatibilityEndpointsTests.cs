using System.Net;
using System.Net.Http.Json;
using Hospital.Data.Models;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class BloodCompatibilityEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task GetTopDonors_ForRecipientWithBloodType_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.PostAsJsonAsync("/api/bloodcompatibilities/top-donors",
            new GetTopCompatibleDonorsRequest { RecipientId = Ids.ActivePatientId });

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var donors = await response.Content.ReadFromJsonAsync<List<Patient>>(JsonOptions);
        Assert.IsNotNull(donors);
    }

    [TestMethod]
    public async Task GetTopDonors_NoLivingDonorsMatch_ReturnsEmptyList()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        // The seeded recipient has a blood type, but there are no deceased,
        // compatible donors, so the service returns an empty list.
        var donors = await client.PostAsJsonAsync("/api/bloodcompatibilities/top-donors",
            new GetTopCompatibleDonorsRequest { RecipientId = Ids.ActivePatientId });
        var result = await donors.Content.ReadFromJsonAsync<List<Patient>>(JsonOptions);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result!);
    }

    [TestMethod]
    public async Task GetTopDonors_AsPharmacist_ReturnsForbidden()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.PharmacistEmail);

        var response = await client.PostAsJsonAsync("/api/bloodcompatibilities/top-donors",
            new GetTopCompatibleDonorsRequest { RecipientId = Ids.ActivePatientId });

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetTopDonors_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/bloodcompatibilities/top-donors",
            new GetTopCompatibleDonorsRequest { RecipientId = Ids.ActivePatientId });

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
