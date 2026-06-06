using System.Net;
using System.Net.Http.Json;

namespace Hospital.Tests.Integration;

[TestClass]
[DoNotParallelize]
public sealed class GhostEndpointsTests : IntegrationTestBase
{
    [TestMethod]
    public async Task ExorcismStatus_IsAnonymous_ReturnsOk()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/ghost/exorcism-status");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task ReportSighting_IncrementsCount()
    {
        using var client = CreateClient();

        var before = await client.GetFromJsonAsync<GhostStatus>("/api/ghost/exorcism-status", JsonOptions);
        var afterResponse = await client.PostAsync("/api/ghost/sighting", content: null);
        var after = await afterResponse.Content.ReadFromJsonAsync<GhostStatus>(JsonOptions);

        Assert.AreEqual(HttpStatusCode.OK, afterResponse.StatusCode);
        Assert.AreEqual(before!.SightingCount + 1, after!.SightingCount);
    }

    [TestMethod]
    public async Task ReportSighting_AboveThreshold_TriggersExorcism()
    {
        using var client = CreateClient();

        GhostStatus? status = null;
        // The threshold is 3 sightings within 24h; report enough to exceed it.
        for (var i = 0; i < 5; i++)
        {
            var response = await client.PostAsync("/api/ghost/sighting", content: null);
            status = await response.Content.ReadFromJsonAsync<GhostStatus>(JsonOptions);
        }

        Assert.IsNotNull(status);
        Assert.IsTrue(status!.ExorcismTriggered);
    }

    private sealed record GhostStatus(bool ExorcismTriggered, int SightingCount);
}
