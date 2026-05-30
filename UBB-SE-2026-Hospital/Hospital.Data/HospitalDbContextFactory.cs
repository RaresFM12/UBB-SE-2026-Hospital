using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hospital.Data;

public class HospitalDbContextFactory : IDesignTimeDbContextFactory<HospitalDbContext>
{
    private const string LocalDbConnectionString = "Data Source=.;Initial Catalog=HospitalDatabase;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";

    public HospitalDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<HospitalDbContext>()
            .UseSqlServer(LocalDbConnectionString)
            .Options;

        return new HospitalDbContext(options);
    }
}
