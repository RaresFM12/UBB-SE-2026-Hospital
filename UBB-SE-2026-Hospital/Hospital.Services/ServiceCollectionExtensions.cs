using Hospital.Services;
using Hospital.Services;
using Hospital.Services;
using Hospital.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Hospital.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHospitalServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IModuleAccessService, ModuleAccessService>();
        services.AddSingleton<IRolePermissionCache, RolePermissionCache>();

        services.AddScoped<Hospital.Shared.Services.IStatisticsService, StatisticsService>();
        services.AddScoped<Hospital.Shared.Services.IAllergyService, AllergyService>();
        services.AddScoped<Hospital.Shared.Services.IBillingService, BillingService>();
        services.AddScoped<Hospital.Shared.Services.IBloodCompatibilityService, BloodCompatibilityService>();
        services.AddScoped<Hospital.Shared.Services.IPrescriptionService, PrescriptionService>();
        services.AddScoped<Hospital.Shared.Services.ITransferLogService, TransferLogService>();
        services.AddScoped<Hospital.Shared.Services.ITransplantService, TransplantService>();
        services.AddScoped<Hospital.Shared.Services.IAddictDetectionService, AddictDetectionService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IERRoomService, ERRoomService>();
        services.AddScoped<IERVisitService, ERVisitService>();
        services.AddScoped<ITriageService, TriageService>();
        services.AddScoped<ITriageParametersService, TriageParametersService>();
        services.AddScoped<ITriageDecisionService, TriageDecisionService>();
        services.AddScoped<IExaminationService, ExaminationService>();

        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IProductCatalogueService, AdminService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IBasketService, BasketService>();
        services.AddScoped<IUserAccountService, UserAccountService>();
        services.AddScoped<IShiftManagementService, ShiftManagementService>();
        services.AddScoped<ISalaryComputationService, SalaryComputationService>();
        services.AddScoped<IPharmacyScheduleService, PharmacyScheduleService>();
        services.AddScoped<IFatigueAuditService, FatigueAuditService>();
        services.AddScoped<IShiftSwapService, ShiftSwapService>();
        services.AddScoped<IDoctorAppointmentService, AppointmentService>();
        services.AddScoped<IMedicalEvaluationService, MedicalEvaluationService>();
        services.AddScoped<IERDispatchService, ERDispatchService>();
        services.AddScoped<IHangoutService, HangoutService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IPharmacyHandoverService, PharmacyHandoverService>();
        services.AddScoped<IPeriodTrackerService, PeriodTrackerService>();
        services.AddScoped<IWellnessItemsService, WellnessItemsService>();
        services.AddScoped<IPharmacyVacationService, PharmacyVacationService>();
        services.AddScoped<IPeriodTrackerService, PeriodTrackerService>();
        services.AddScoped<IWellnessItemsService, WellnessItemsService>();
        services.AddScoped<ISalaryComputationService, SalaryComputationService>();
        services.AddScoped<IPharmacyScheduleService, PharmacyScheduleService>();
        services.AddScoped<IPharmacyVacationService, PharmacyVacationService>();


        return services;
    }
}
