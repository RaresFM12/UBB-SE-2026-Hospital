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

public class TriageParametersEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient httpClient;

    public TriageParametersEndpointTests(TestWebApplicationFactory factory)
    {
        httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetTriageParametersById_WhenRecordDoesNotExist_Returns_NotFound()
    {
        int nonExistentTriageParametersId = 999999;

        var httpResponse = await httpClient.GetAsync($"/api/triageparameters/{nonExistentTriageParametersId}");

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task GetTriageParametersByTriageId_WhenTriageHasNoParameters_Returns_NotFound()
    {
        int triageIdWithNoParameters = 888888;

        var httpResponse = await httpClient.GetAsync($"/api/triageparameters/triage/{triageIdWithNoParameters}");

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteTriageParameters_WhenRecordDoesNotExist_Returns_NotFound()
    {
        int nonExistentTriageParametersId = 777777;

        var httpResponse = await httpClient.DeleteAsync($"/api/triageparameters/{nonExistentTriageParametersId}");

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }
}
