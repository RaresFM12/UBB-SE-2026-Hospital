using System.Net;
using System.Net.Http.Json;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class StatisticsEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task ActiveVsArchived_ReflectsSeededPatients()
    {
        using var client = await CreateAdminClientAsync();

        var ratio = await client.GetFromJsonAsync<Dictionary<string, int>>(
            "/api/statistics/active-vs-archived", JsonOptions);

        Assert.IsNotNull(ratio);
        Assert.IsTrue(ratio!["Active"] >= 1);
        Assert.IsTrue(ratio["Archived"] >= 1);
    }

    [TestMethod]
    public async Task ActiveVsArchived_AsDoctor_ReturnsForbidden()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync("/api/statistics/active-vs-archived");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task AgeDistribution_ContainsAllBuckets()
    {
        using var client = await CreateAdminClientAsync();

        var distribution = await client.GetFromJsonAsync<Dictionary<string, int>>(
            "/api/statistics/age-distribution", JsonOptions);

        Assert.IsNotNull(distribution);
        Assert.IsTrue(distribution!.ContainsKey("Pediatric (0-17)"));
        Assert.IsTrue(distribution.ContainsKey("Adult (18-64)"));
        Assert.IsTrue(distribution.ContainsKey("Geriatric (65+)"));
    }

    [TestMethod]
    public async Task BloodTypes_GroupsSeededPatients()
    {
        using var client = await CreateAdminClientAsync();

        var byBloodType = await client.GetFromJsonAsync<Dictionary<string, int>>(
            "/api/statistics/blood-types", JsonOptions);

        Assert.IsNotNull(byBloodType);
        // Seeded patients have blood types O and A.
        Assert.IsTrue(byBloodType!.ContainsKey("O"));
        Assert.IsTrue(byBloodType.ContainsKey("A"));
    }

    [TestMethod]
    public async Task RhFactor_GroupsSeededPatients()
    {
        using var client = await CreateAdminClientAsync();

        var byRh = await client.GetFromJsonAsync<Dictionary<string, int>>(
            "/api/statistics/rh-factor", JsonOptions);

        Assert.IsNotNull(byRh);
        Assert.IsTrue(byRh!.ContainsKey("Positive") || byRh.ContainsKey("Negative"));
    }

    [TestMethod]
    public async Task GenderDistribution_CountsBothSexes()
    {
        using var client = await CreateAdminClientAsync();

        var byGender = await client.GetFromJsonAsync<Dictionary<string, int>>(
            "/api/statistics/gender-distribution", JsonOptions);

        Assert.IsNotNull(byGender);
        // Seeded patients: one F (Ana) and one M (Vlad).
        Assert.IsTrue(byGender!.ContainsKey("F"));
        Assert.IsTrue(byGender.ContainsKey("M"));
    }

    [TestMethod]
    public async Task Consultations_ReturnsOk()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/statistics/consultations");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task TopDiagnoses_ReturnsOk()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/statistics/top-diagnoses");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task TopMeds_ReturnsOk()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/statistics/top-meds");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
