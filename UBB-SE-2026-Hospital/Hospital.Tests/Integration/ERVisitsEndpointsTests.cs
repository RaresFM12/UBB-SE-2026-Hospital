using System.Net;
using System.Net.Http.Json;
using Hospital.Data.Models;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class ERVisitsEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task GetAll_AsDoctor_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync("/api/ervisits");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<ERVisit>>(JsonOptions);
        Assert.IsNotNull(items);
    }

    [TestMethod]
    public async Task GetActive_AsNurse_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.NurseEmail);

        var response = await client.GetAsync("/api/ervisits/active");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetByPatientId_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync($"/api/ervisits/patient/{Ids.ActivePatientId}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetById_Missing_ReturnsNotFound()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync("/api/ervisits/999999");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task AutoAssignRoom_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.PostAsync("/api/ervisits/auto-assign-room", content: null);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_AsPharmacist_ReturnsForbidden()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.PharmacistEmail);

        var response = await client.GetAsync("/api/ervisits");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/ervisits");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
