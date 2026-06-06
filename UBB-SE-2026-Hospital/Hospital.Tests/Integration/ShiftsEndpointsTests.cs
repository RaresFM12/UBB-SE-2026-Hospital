using System.Net;
using System.Net.Http.Json;
using Hospital.Data.Models;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class ShiftsEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task GetAll_AsNurse_ReturnsSeededShift()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.NurseEmail);

        var shifts = await client.GetFromJsonAsync<List<Shift>>("/api/shifts", JsonOptions);

        Assert.IsNotNull(shifts);
        Assert.IsTrue(shifts!.Any(s => s.Id == Ids.ShiftId));
    }

    [TestMethod]
    public async Task GetAll_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/shifts");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetById_Existing_ReturnsShift()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var shift = await client.GetFromJsonAsync<Shift>($"/api/shifts/{Ids.ShiftId}", JsonOptions);

        Assert.IsNotNull(shift);
        Assert.AreEqual("Diagnostics", shift!.Location);
    }

    [TestMethod]
    public async Task GetById_Missing_ReturnsNotFound()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);

        var response = await client.GetAsync("/api/shifts/777777");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Create_NewShift_PersistsIt()
    {
        using var client = await CreateAdminClientAsync();
        var request = new
        {
            StaffId = Ids.PharmacistStaffId,
            Location = "Pharmacy",
            StartTime = new DateTime(2025, 2, 1, 9, 0, 0),
            EndTime = new DateTime(2025, 2, 1, 17, 0, 0),
            Status = ShiftStatus.Scheduled,
        };

        var response = await client.PostAsJsonAsync("/api/shifts", request);

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        var shifts = await client.GetFromJsonAsync<List<Shift>>("/api/shifts", JsonOptions);
        Assert.IsTrue(shifts!.Any(s => s.Location == "Pharmacy"));
    }

    [TestMethod]
    public async Task Create_OverlappingShift_ThrowsInvalidOperation()
    {
        using var client = await CreateAdminClientAsync();
        var request = new
        {
            StaffId = Ids.DoctorStaffId,
            Location = "Diagnostics",
            // Overlaps the seeded 08:00-16:00 shift for the same doctor.
            StartTime = new DateTime(2025, 1, 6, 10, 0, 0),
            EndTime = new DateTime(2025, 1, 6, 12, 0, 0),
            Status = ShiftStatus.Scheduled,
        };

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => client.PostAsJsonAsync("/api/shifts", request));
    }

    [TestMethod]
    public async Task UpdateStatus_ChangesShiftStatus()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.PatchAsJsonAsync(
            $"/api/shifts/{Ids.ShiftId}/status", new { Status = ShiftStatus.Active });

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        var shift = await client.GetFromJsonAsync<Shift>($"/api/shifts/{Ids.ShiftId}", JsonOptions);
        Assert.AreEqual(ShiftStatus.Active, shift!.Status);
    }

    [TestMethod]
    public async Task Delete_ExistingShift_RemovesIt()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.DeleteAsync($"/api/shifts/{Ids.ShiftId}");

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        var lookup = await client.GetAsync($"/api/shifts/{Ids.ShiftId}");
        Assert.AreEqual(HttpStatusCode.NotFound, lookup.StatusCode);
    }
}
