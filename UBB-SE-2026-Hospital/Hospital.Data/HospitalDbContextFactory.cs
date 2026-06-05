using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hospital.Data;

public class HospitalDbContextFactory : IDesignTimeDbContextFactory<HospitalDbContext>
{
    private const string LocalDbConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=HospitalDatabase;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";

    public HospitalDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<HospitalDbContext>()
            .UseSqlServer(LocalDbConnectionString)
            .Options;

        return new HospitalDbContext(options);
    }
}
