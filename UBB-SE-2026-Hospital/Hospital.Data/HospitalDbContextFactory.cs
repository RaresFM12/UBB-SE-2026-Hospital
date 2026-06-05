using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hospital.Data;

public class HospitalDbContextFactory : IDesignTimeDbContextFactory<HospitalDbContext>
{
    private const string LocalDbConnectionString = "Server=DESKTOP-G90IV3T\\MSSQLSERVER01;Database=HospitalDB;Trusted_Connection=True;TrustServerCertificate=True;";

    public HospitalDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<HospitalDbContext>()
            .UseSqlServer(LocalDbConnectionString)
            .Options;

        return new HospitalDbContext(options);
    }
}
