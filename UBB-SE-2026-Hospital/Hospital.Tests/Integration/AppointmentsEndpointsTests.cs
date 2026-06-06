using System.Net;
using System.Net.Http.Json;
using Hospital.Data.Models;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class AppointmentsEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task GetAll_AsDoctor_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync("/api/appointments");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var appointments = await response.Content.ReadFromJsonAsync<List<Appointment>>(JsonOptions);
        Assert.IsNotNull(appointments);
    }

    [TestMethod]
    public async Task GetAll_AsNurse_ReturnsForbidden()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.NurseEmail);

        var response = await client.GetAsync("/api/appointments");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/appointments");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task Create_WithUnknownDoctor_ThrowsArgumentException()
    {
        using var client = await CreateAdminClientAsync();
        var request = new
        {
            PatientId = Ids.ActivePatientId,
            DoctorId = 999999,
            StartTime = new DateTime(2025, 5, 1, 9, 0, 0),
            EndTime = new DateTime(2025, 5, 1, 10, 0, 0),
            Status = "Scheduled",
        };

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => client.PostAsJsonAsync("/api/appointments", request));
    }
}
