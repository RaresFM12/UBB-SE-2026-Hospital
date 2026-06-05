using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hospital.Data.Models;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class PatientsEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task GetAll_AsDoctor_ReturnsSeededPatients()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var patients = await client.GetFromJsonAsync<List<Patient>>("/api/patients", JsonOptions);

        Assert.IsNotNull(patients);
        Assert.IsTrue(patients!.Count >= 2);
        Assert.IsTrue(patients.Any(p => p.FirstName == "Ana" && p.LastName == "Pop"));
    }

    [TestMethod]
    public async Task GetAll_AsNurse_IsAllowed()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.NurseEmail);

        var response = await client.GetAsync("/api/patients");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/patients");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_AsPharmacist_ReturnsForbidden()
    {
        // Pharmacist is authenticated but not in the allowed role list.
        using var client = await CreateAuthenticatedClientAsync(SeededIds.PharmacistEmail);

        var response = await client.GetAsync("/api/patients");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_WithTamperedToken_ReturnsUnauthorized()
    {
        var token = await LoginAsync(SeededIds.DoctorEmail);
        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token + "tampered");

        var response = await client.GetAsync("/api/patients");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
