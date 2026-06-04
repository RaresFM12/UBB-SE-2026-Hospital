using Hospital.Tests.Integration.PatientER;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;


namespace Hospital.Tests.Integration.PatientER;

public class UsersEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient httpClient;

    public UsersEndpointTests(TestWebApplicationFactory factory)
    {
        httpClient = factory.CreateClient();
    }

}

