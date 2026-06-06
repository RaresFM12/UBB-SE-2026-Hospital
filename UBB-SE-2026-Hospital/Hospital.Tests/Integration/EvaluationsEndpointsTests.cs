using System.Net;
using System.Net.Http.Json;
using Hospital.Data.Models;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class EvaluationsEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task GetAll_AsDoctor_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync("/api/evaluations");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<MedicalEvaluation>>(JsonOptions);
        Assert.IsNotNull(items);
    }

    [TestMethod]
    public async Task GetAll_AsAdmin_ReturnsOk()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/evaluations");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_AsNurse_ReturnsForbidden()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.NurseEmail);

        var response = await client.GetAsync("/api/evaluations");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/evaluations");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task Update_NonExistent_ThrowsArgumentException()
    {
        using var client = await CreateAdminClientAsync();
        var request = new { Diagnosis = "Flu", Notes = "Rest", Medications = "Paracetamol" };

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => client.PutAsJsonAsync("/api/evaluations/999999", request));
    }

    [TestMethod]
    public async Task Delete_NonExistent_ReturnsNoContent()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.DeleteAsync("/api/evaluations/999999");

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }
}
