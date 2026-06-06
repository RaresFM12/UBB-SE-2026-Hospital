using System.Net;
using System.Net.Http.Json;
using Hospital.Data.Models;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class StaffEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task GetAll_AsAdmin_ReturnsSeededStaff()
    {
        using var client = await CreateAdminClientAsync();

        var staff = await client.GetFromJsonAsync<List<Staff>>("/api/staff", JsonOptions);

        Assert.IsNotNull(staff);
        Assert.IsTrue(staff!.Count >= 2);
    }

    [TestMethod]
    public async Task GetAll_AsNurse_ReturnsForbidden()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.NurseEmail);

        var response = await client.GetAsync("/api/staff");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetById_Existing_ReturnsStaffMember()
    {
        using var client = await CreateAdminClientAsync();

        var staff = await client.GetFromJsonAsync<Staff>($"/api/staff/{Ids.DoctorStaffId}", JsonOptions);

        Assert.IsNotNull(staff);
        Assert.AreEqual("House", staff!.LastName);
    }

    [TestMethod]
    public async Task GetById_Missing_ReturnsNotFound()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/staff/123456");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task GetDoctors_ReturnsSeededDoctor()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/staff/doctors");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var doctors = await response.Content.ReadFromJsonAsync<List<DoctorSummary>>(JsonOptions);
        Assert.IsTrue(doctors!.Any(d => d.LastName == "House"));
    }

    [TestMethod]
    public async Task GetPharmacists_ReturnsSeededPharmacist()
    {
        using var client = await CreateAdminClientAsync();

        var pharmacists = await client.GetFromJsonAsync<List<Pharmacyst>>("/api/staff/pharmacists", JsonOptions);

        Assert.IsTrue(pharmacists!.Any(p => p.LastName == "Mortar"));
    }

    [TestMethod]
    public async Task UpdateStatus_ChangesStaffStatus()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.PatchAsJsonAsync(
            $"/api/staff/{Ids.PharmacistStaffId}/status", new { Status = "OffDuty" });

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        var staff = await client.GetFromJsonAsync<Staff>($"/api/staff/{Ids.PharmacistStaffId}", JsonOptions);
        Assert.AreEqual("OffDuty", staff!.Status);
    }

    [TestMethod]
    public async Task UpdateAvailability_ChangesAvailability()
    {
        using var client = await CreateAdminClientAsync();

        var response = await client.PatchAsJsonAsync(
            $"/api/staff/{Ids.DoctorStaffId}/availability",
            new { IsAvailable = false, Status = DoctorStatus.OffDuty });

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    private sealed record DoctorSummary(int DoctorId, string FirstName, string LastName);
}
