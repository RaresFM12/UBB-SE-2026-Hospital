using Hospital.Services.Auth;
using Hospital.Services.PatientEr;
using Hospital.Services.StaffPharmacy;
using Hospital.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Hospital.Services.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHospitalServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IModuleAccessService, ModuleAccessService>();
        services.AddSingleton<IRolePermissionCache, RolePermissionCache>();

        services.AddScoped<Hospital.Shared.Services.IStatisticsService, Hospital.Services.PatientEr.StatisticsService>();
        services.AddScoped<IAllergyService, AllergyService>();
        services.AddScoped<Hospital.Shared.Services.IBillingService, BillingService>();
        services.AddScoped<IBloodCompatibilityService, BloodCompatibilityService>();
        services.AddScoped<IPrescriptionService, PrescriptionService>();
        services.AddScoped<ITransferLogService, TransferLogService>();
        services.AddScoped<ITransplantService, TransplantService>();
        services.AddScoped<IAddictDetectionService, AddictDetectionService>();
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
