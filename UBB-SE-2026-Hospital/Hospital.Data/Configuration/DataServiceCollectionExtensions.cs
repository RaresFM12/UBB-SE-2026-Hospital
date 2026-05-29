using Hospital.Data.Repositories;
using Hospital.Shared.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hospital.Data.Configuration;

public static class DataServiceCollectionExtensions
{
    public static IServiceCollection AddHospitalData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<HospitalDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();

        return services;
    }
}
