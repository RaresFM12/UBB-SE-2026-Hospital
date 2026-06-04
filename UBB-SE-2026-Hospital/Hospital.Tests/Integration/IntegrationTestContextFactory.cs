using System;
using Hospital.Data;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Tests.Integration
{
    internal static class IntegrationTestContextFactory
    {
        public static HospitalDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<HospitalDbContext>()
                .UseInMemoryDatabase(databaseName: $"HospitalIntegration_{Guid.NewGuid()}")
                .EnableSensitiveDataLogging()
                .Options;

            return new HospitalDbContext(options);
        }
    }
}
