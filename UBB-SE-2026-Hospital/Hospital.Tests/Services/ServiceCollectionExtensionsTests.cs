using Hospital.Services;
using Hospital.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Hospital.Tests.Services;

[TestClass]
public class ServiceCollectionExtensionsTests
{
    [TestMethod]
    public void AddHospitalServices_ReturnsSameCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddHospitalServices();

        Assert.AreSame(services, result);
    }

    [TestMethod]
    public void AddHospitalServices_RegistersPatientService()
    {
        var services = new ServiceCollection();

        services.AddHospitalServices();

        Assert.IsTrue(services.Any(descriptor => descriptor.ServiceType == typeof(IPatientService)));
    }
}
