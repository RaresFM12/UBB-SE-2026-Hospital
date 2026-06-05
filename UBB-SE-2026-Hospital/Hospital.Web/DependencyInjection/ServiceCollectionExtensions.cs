using Hospital.Web.Services;
using Hospital.Shared.Proxies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hospital.Web.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHospitalWebServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string apiBaseUrl = configuration["ApiBaseUrl"]
            ?? throw new InvalidOperationException("Missing configuration value: ApiBaseUrl.");

        services.AddHttpContextAccessor();
        services.AddTransient<AuthTokenForwardingHandler>();

        services.AddSingleton<IErStaffService, ErStaffService>();
        services.AddSingleton<IAppointmentImportProvider, MockAppointmentImportProvider>();

        services.AddHospitalApiClient<IAuthenticationApiClient, AuthenticationApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IAddictDetectionApiClient, AddictDetectionApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IAllergyApiClient, AllergyApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IBillingApiClient, BillingApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IBloodCompatibilityApiClient, BloodCompatibilityApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IErWorkflowApiClient, ErWorkflowApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IGhostApiClient, GhostApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IPatientApiClient, PatientApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IPrescriptionApiClient, PrescriptionApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IStatisticsApiClient, StatisticsApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<ITransplantApiClient, TransplantApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IAdminApiClient, AdminApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IDoctorAppointmentApiClient, DoctorAppointmentApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IUserAccountApiClient, UserAccountApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IShiftManagementApiClient, ShiftManagementApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IOrdersApiClient, OrdersApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IFatigueAuditApiClient, FatigueAuditApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IERDispatchApiClient, ERDispatchApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IERVisitApiClient, ERVisitApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IHangoutApiClient, HangoutApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IBasketApiClient, BasketApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IMedicalEvaluationApiClient, MedicalEvaluationApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IShiftSwapApiClient, ShiftSwapApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IPharmacyScheduleApiClient, PharmacyScheduleApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IPharmacyVacationApiClient, PharmacyVacationApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IProductCatalogueApiClient, ProductCatalogueApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IWellnessItemsApiClient, WellnessItemsApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<IPeriodTrackerApiClient, PeriodTrackerApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<ISalaryComputationApiClient, SalaryComputationApiClient>(apiBaseUrl);
        services.AddHospitalApiClient<ITriageDecisionApiClient, TriageDecisionApiClient>(apiBaseUrl);

        return services;
    }

    private static IServiceCollection AddHospitalApiClient<TService, TImplementation>(
        this IServiceCollection services,
        string apiBaseUrl)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddHttpClient<TService, TImplementation>(client =>
            client.BaseAddress = new Uri(apiBaseUrl))
            .AddHttpMessageHandler<AuthTokenForwardingHandler>();

        return services;
    }
}
