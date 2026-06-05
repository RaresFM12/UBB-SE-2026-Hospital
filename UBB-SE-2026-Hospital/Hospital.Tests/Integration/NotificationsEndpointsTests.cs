using System.Net;
using System.Net.Http.Json;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class NotificationsEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task Create_WithKnownStaff_ReturnsNoContent()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);
        var request = new { RecipientStaffId = Ids.DoctorStaffId, Title = "Reminder", Message = "Shift starts soon." };

        var response = await client.PostAsJsonAsync("/api/notifications", request);

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    [TestMethod]
    public async Task Create_WithUnknownStaff_ThrowsArgumentException()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.DoctorEmail);
        var request = new { RecipientStaffId = 999999, Title = "Reminder", Message = "Hello." };

        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => client.PostAsJsonAsync("/api/notifications", request));
    }

    [TestMethod]
    public async Task Create_AsClient_ReturnsForbidden()
    {
        using var client = await CreateAuthenticatedClientAsync(SeededIds.ClientEmail);
        var request = new { RecipientStaffId = Ids.DoctorStaffId, Title = "Reminder", Message = "Hello." };

        var response = await client.PostAsJsonAsync("/api/notifications", request);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task Create_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();
        var request = new { RecipientStaffId = Ids.DoctorStaffId, Title = "Reminder", Message = "Hello." };

        var response = await client.PostAsJsonAsync("/api/notifications", request);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
