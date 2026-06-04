using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;

namespace Hospital.Tests.Integration.PatientER
{
    public class AuthModulesEndpointTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient httpClient;

        public AuthModulesEndpointTests(TestWebApplicationFactory factory)
        {
            httpClient = factory.CreateClient();
        }

        [Fact]
        public async Task GetModules_WhenNoAuthorizationHeaderIsProvided_Returns_Unauthorized()
        {
            var httpResponse = await httpClient.GetAsync("/api/auth/modules");

            Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
        }

        [Fact]
        public async Task CanAccessModule_WhenNoAuthorizationHeaderIsProvided_Returns_Unauthorized()
        {
            var httpResponse = await httpClient.GetAsync("/api/auth/modules/triage/access");

            Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
        }
    }
}
