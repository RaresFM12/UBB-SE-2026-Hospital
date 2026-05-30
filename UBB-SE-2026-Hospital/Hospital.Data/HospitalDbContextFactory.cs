using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hospital.Data;

public class HospitalDbContextFactory : IDesignTimeDbContextFactory<HospitalDbContext>
{
    public HospitalDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<HospitalDbContext>()
            .UseSqlServer("Data Source=.;Initial Catalog=HospitalDatabase;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;")
            .Options;

        return new HospitalDbContext(options);
    }
}
