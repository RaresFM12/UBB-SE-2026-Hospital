using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hospital.Data;

public class HospitalDbContextFactory : IDesignTimeDbContextFactory<HospitalDbContext>
{
    private const string LocalDbConnectionString = "Data Source=.;Initial Catalog=HospitalDatabase;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";
    private const string ConnectionStringEnvironmentVariable = "ConnectionStrings__DefaultConnection";

    public HospitalDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable)
            ?? LocalDbConnectionString;

        var options = new DbContextOptionsBuilder<HospitalDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new HospitalDbContext(options);
    }
}
