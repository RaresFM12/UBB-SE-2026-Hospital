
using System.Net;
using System.Net.Http.Json;
using Hospital.Data.Models;
using Hospital.Shared.DTOs.Auth;
using Xunit;
using Assert = Xunit.Assert;

namespace Hospital.Tests.Integration.PatientER;

public class AuthLoginEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient httpClient;

    public AuthLoginEndpointTests(TestWebApplicationFactory factory)
    {
        httpClient = factory.CreateClient();
    }

    
    [Fact]
    public async Task Login_WhenRequestBodyIsEmpty_Returns_BadRequestOrUnprocessable()
    {
        var emptyJsonContent = new StringContent(
            string.Empty,
            System.Text.Encoding.UTF8,
            "application/json");

        var httpResponse = await httpClient.PostAsync("/api/auth/login", emptyJsonContent);

        Assert.True(
            httpResponse.StatusCode == HttpStatusCode.BadRequest ||
            httpResponse.StatusCode == HttpStatusCode.UnprocessableEntity);
    }
}