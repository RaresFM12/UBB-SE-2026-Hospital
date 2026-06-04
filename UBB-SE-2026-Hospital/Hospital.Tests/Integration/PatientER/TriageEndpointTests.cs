using Hospital.Tests.Integration.PatientER;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;

namespace Hospital.Tests.Integration.PatientER;

public class TriageEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient httpClient;

    public TriageEndpointTests(TestWebApplicationFactory factory)
    {
        httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetTriageById_WhenTriageDoesNotExist_Returns_NotFound()
    {
        int nonExistentTriageId = 999999;

        var httpResponse = await httpClient.GetAsync($"/api/triage/{nonExistentTriageId}");

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }


    [Fact]
    public async Task DeleteTriage_WhenTriageDoesNotExist_Returns_NotFound()
    {
        int nonExistentTriageId = 666666;

        var httpResponse = await httpClient.DeleteAsync($"/api/triage/{nonExistentTriageId}");

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }
}

