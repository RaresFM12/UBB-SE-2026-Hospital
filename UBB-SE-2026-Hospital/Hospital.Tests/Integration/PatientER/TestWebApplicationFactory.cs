using Hospital.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hospital.Tests.Integration.PatientER;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove every descriptor that references HospitalDbContext or its options,
            // including the SQL Server internal service provider registered by AddHospitalData.
            var hospitalDbContextDescriptors = services
                .Where(serviceDescriptor =>
                    serviceDescriptor.ServiceType == typeof(HospitalDbContext) ||
                    serviceDescriptor.ServiceType == typeof(DbContextOptions<HospitalDbContext>) ||
                    serviceDescriptor.ServiceType == typeof(DbContextOptions))
                .ToList();

            foreach (var hospitalDbContextDescriptor in hospitalDbContextDescriptors)
            {
                services.Remove(hospitalDbContextDescriptor);
            }

            // Register HospitalDbContext backed by a unique in-memory database per test run.
            services.AddDbContext<HospitalDbContext>(dbContextOptions =>
            {
                dbContextOptions.UseInMemoryDatabase("IntegrationTestDb_" + Guid.NewGuid());
            });
        });
    }
}