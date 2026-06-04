using System.Net;
using System.Net.Http.Json;
using Hospital.Data.Models;
using Xunit;
using Assert = Xunit.Assert;

namespace Hospital.Tests.Integration.PatientER;

public class AuthEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient httpClient;

    public AuthEndpointTests(TestWebApplicationFactory factory)
    {
        httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WhenRequestBodyIsEmpty_Returns_BadRequestOrUnprocessable()
    {
        var emptyContent = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

        var httpResponse = await httpClient.PostAsync("/api/auth/login", emptyContent);

        Assert.True(
            httpResponse.StatusCode == HttpStatusCode.BadRequest ||
            httpResponse.StatusCode == HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task GetModules_WhenCalledWithoutAuthToken_Returns_Unauthorized()
    {
        var httpResponse = await httpClient.GetAsync("/api/auth/modules");

        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
    }
}
