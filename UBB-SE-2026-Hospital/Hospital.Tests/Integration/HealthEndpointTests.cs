using System.Net;
using System.Net.Http.Json;

namespace Hospital.Tests.Integration;

[TestClass]
public sealed class HealthEndpointTests : IntegrationTestBase
{
    [TestMethod]
    public async Task Get_ReturnsOk()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/health");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task Get_IsAnonymous_NoTokenRequired()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/health");
        var payload = await response.Content.ReadFromJsonAsync<HealthPayload>(JsonOptions);

        Assert.IsNotNull(payload);
        Assert.AreEqual("ok", payload!.Status);
    }

    [TestMethod]
    public async Task Get_ReportsSolutionName()
    {
        using var client = CreateClient();

        var payload = await client.GetFromJsonAsync<HealthPayload>("/api/health", JsonOptions);

        Assert.IsNotNull(payload);
        Assert.AreEqual("UBB-SE-2026-Hospital", payload!.Solution);
    }

    private sealed record HealthPayload(string Status, string Solution, string Message);
}
