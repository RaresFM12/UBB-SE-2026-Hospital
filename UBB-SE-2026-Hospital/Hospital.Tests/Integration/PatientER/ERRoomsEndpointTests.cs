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

public class ERRoomsEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient httpClient;

    public ERRoomsEndpointTests(TestWebApplicationFactory factory)
    {
        httpClient = factory.CreateClient();
    }


    [Fact]
    public async Task GetERRoomById_WhenRoomDoesNotExist_Returns_NotFound()
    {
        int nonExistentRoomId = 999999;

        var httpResponse = await httpClient.GetAsync($"/api/errooms/{nonExistentRoomId}");

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task GetVisitDetails_WhenRoomDoesNotExist_Returns_NotFound()
    {
        int nonExistentRoomId = 888888;

        var httpResponse = await httpClient.GetAsync($"/api/errooms/{nonExistentRoomId}/visit-details");

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteERRoom_WhenRoomDoesNotExist_Returns_NotFound()
    {
        int nonExistentRoomId = 777777;

        var httpResponse = await httpClient.DeleteAsync($"/api/errooms/{nonExistentRoomId}");

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }
}
