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

        // Patient/ER services
        services.AddScoped<IAllergyService, AllergyService>();
        services.AddScoped<IBillingService, BillingService>();
        services.AddScoped<IBloodCompatibilityService, BloodCompatibilityService>();
        services.AddScoped<IPrescriptionService, PrescriptionService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<ITransferLogService, TransferLogService>();
        services.AddScoped<ITransplantService, TransplantService>();
        services.AddScoped<IAddictDetectionService, AddictDetectionService>();

        return services;
    }
}
