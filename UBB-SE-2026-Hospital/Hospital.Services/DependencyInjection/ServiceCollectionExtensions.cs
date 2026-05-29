using Hospital.Shared.Services;
using Hospital.Services.Auth;
using Hospital.Services.PatientEr;
using Hospital.Services.StaffPharmacy;
using Microsoft.Extensions.DependencyInjection;

namespace Hospital.Services.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHospitalServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IPatientService, PatientService>();

        return services;
    }
}
