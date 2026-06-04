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


public class ERVisitsEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient httpClient;

    public ERVisitsEndpointTests(TestWebApplicationFactory factory)
    {
        httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetERVisitById_WhenVisitDoesNotExist_Returns_NotFound()
    {
        int nonExistentVisitId = 999999;

        var httpResponse = await httpClient.GetAsync($"/api/ervisits/{nonExistentVisitId}");

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteERVisit_WhenVisitDoesNotExist_Returns_NotFound()
    {
        int nonExistentVisitId = 777777;

        var httpResponse = await httpClient.DeleteAsync($"/api/ervisits/{nonExistentVisitId}");

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }
}