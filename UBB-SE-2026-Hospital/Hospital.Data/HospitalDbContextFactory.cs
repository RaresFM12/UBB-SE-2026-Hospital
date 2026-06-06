using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hospital.Data;

public class HospitalDbContextFactory : IDesignTimeDbContextFactory<HospitalDbContext>
{
    // private const string LocalDbConnectionString = "Server=.\\MSSQLSERVER01;Database=HospitalDB;Trusted_
    // ion=True;TrustServerCertificate=True;";
    private const string LocalDbConnectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=HospitalDatabase;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";

    public HospitalDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<HospitalDbContext>()
            .UseSqlServer(LocalDbConnectionString)
            .Options;

        return new HospitalDbContext(options);
    }
}
