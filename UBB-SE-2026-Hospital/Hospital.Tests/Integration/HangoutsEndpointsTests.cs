using System.Net;
using System.Net.Http.Json;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class HangoutsEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task GetAll_AsDoctor_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync("/api/hangouts");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_AsPharmacist_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.PharmacistEmail);

        var response = await client.GetAsync("/api/hangouts");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetById_Missing_ReturnsNotFound()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync("/api/hangouts/999999");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Create_Valid_ReturnsId()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);
        var request = new
        {
            Title = "Coffee Break",
            Description = "Relax together",
            Date = DateTime.Now.Date.AddDays(14),
            MaxParticipants = 10,
        };

        var response = await client.PostAsJsonAsync("/api/hangouts", request);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var id = await response.Content.ReadFromJsonAsync<int>(JsonOptions);
        Assert.IsGreaterThan(0, id);
    }

    [TestMethod]
    public async Task Create_WithShortTitle_ThrowsArgumentException()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);
        var request = new
        {
            Title = "Hi",
            Description = "Too short title",
            Date = DateTime.Now.Date.AddDays(14),
            MaxParticipants = 10,
        };

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => client.PostAsJsonAsync("/api/hangouts", request));
    }

    [TestMethod]
    public async Task GetAll_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/hangouts");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
