using Hospital.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Hospital.Tests.Integration;

/// <summary>
/// Boots the real Hospital.API pipeline (routing, JWT authentication, DB-backed
/// role authorization, controllers, services and repositories) but swaps the
/// SQL Server database for an isolated SQLite in-memory store. SQLite is a real
/// relational provider, so LINQ (GroupBy/SelectMany/joins) is translated just
/// like production, letting the full request/response stack be exercised without
/// external infrastructure.
/// </summary>
public sealed class HospitalApiFactory : WebApplicationFactory<Program>
{
    // A single shared connection keeps the in-memory database alive for the
    // lifetime of the factory; closing it drops the schema and all data.
    private readonly SqliteConnection connection = new("DataSource=:memory:");

    public SeededIds Ids { get; private set; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        connection.Open();

        builder.ConfigureServices(services =>
        {
            // Strip every EF Core registration tied to the production SqlServer
            // provider. EF Core 9 also adds an IDbContextOptionsConfiguration<T>
            // descriptor (via UseSqlServer), which must be removed too, otherwise
            // two providers end up registered in the same container.
            var descriptorsToRemove = services
                .Where(descriptor =>
                    descriptor.ServiceType == typeof(HospitalDbContext) ||
                    descriptor.ServiceType == typeof(DbContextOptions) ||
                    descriptor.ServiceType == typeof(DbContextOptions<HospitalDbContext>) ||
                    (descriptor.ServiceType.FullName?.Contains("IDbContextOptionsConfiguration") ?? false))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<HospitalDbContext>(options =>
                options.UseSqlite(connection));
        });
    }

    /// <summary>
    /// Forces the host to build, creates the schema (applying EF Core seed data)
    /// and inserts the test fixtures. Returns the identifiers assigned to the
    /// seeded rows so tests can address them precisely.
    /// </summary>
    public SeededIds SeedDatabase()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<HospitalDbContext>();
        context.Database.EnsureCreated();
        Ids = TestSeedData.Seed(context);
        return Ids;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            connection.Dispose();
        }
    }
}
