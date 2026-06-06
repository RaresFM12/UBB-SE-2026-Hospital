using System.Net;
using System.Net.Http.Json;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class ShiftSwapsEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task GetAll_AsDoctor_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync("/api/shift-swaps");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_AsNurse_ReturnsOk()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.NurseEmail);

        var response = await client.GetAsync("/api/shift-swaps");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetById_Missing_ReturnsNotFound()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync("/api/shift-swaps/999999");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Create_WithUnknownShift_ThrowsArgumentException()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);
        var request = new
        {
            ShiftId = 999999,
            RequesterId = Ids.DoctorStaffId,
            ColleagueId = Ids.PharmacistStaffId,
            RequestedAt = DateTime.UtcNow,
            Status = 0,
        };

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => client.PostAsJsonAsync("/api/shift-swaps", request));
    }

    [TestMethod]
    public async Task UpdateStatus_Missing_ThrowsArgumentException()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);
        var request = new { Status = "APPROVED" };

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => client.PatchAsJsonAsync("/api/shift-swaps/999999/status", request));
    }

    [TestMethod]
    public async Task GetAll_AsClient_ReturnsForbidden()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.ClientEmail);

        var response = await client.GetAsync("/api/shift-swaps");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/shift-swaps");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
