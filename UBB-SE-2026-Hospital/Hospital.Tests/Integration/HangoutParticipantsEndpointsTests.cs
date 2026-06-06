using System.Net;
using System.Net.Http.Json;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class HangoutParticipantsEndpointsTests : IntegrationTestBase
{
    private sealed record HangoutParticipantSummary(int HangoutId, int StaffId);

    [TestMethod]
    public async Task GetAll_AsNurse_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.NurseEmail);

        var response = await client.GetAsync("/api/hangout-participants");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<HangoutParticipantSummary>>(JsonOptions);
        Assert.IsNotNull(items);
    }

    [TestMethod]
    public async Task GetAll_AsDoctor_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync("/api/hangout-participants");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task Create_WithUnknownHangout_ThrowsArgumentException()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);
        var request = new { HangoutId = 999999, StaffId = Ids.DoctorStaffId };

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => client.PostAsJsonAsync("/api/hangout-participants", request));
    }

    [TestMethod]
    public async Task GetAll_AsClient_ReturnsForbidden()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.ClientEmail);

        var response = await client.GetAsync("/api/hangout-participants");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/hangout-participants");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
