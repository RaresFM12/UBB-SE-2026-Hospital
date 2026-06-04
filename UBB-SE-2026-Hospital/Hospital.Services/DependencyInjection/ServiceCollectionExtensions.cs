using Hospital.Services.Auth;
using Hospital.Services.PatientEr;
// using Hospital.Services.StaffPharmacy;
using Hospital.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Hospital.Services.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHospitalServices(this IServiceCollection services)
    {
        // Auth
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IModuleAccessService, ModuleAccessService>();
        services.AddSingleton<IRolePermissionCache, RolePermissionCache>();

        // Patient / ER domain (from 926-2)
        services.AddScoped<Hospital.Shared.Services.IAllergyService, AllergyService>();
        services.AddScoped<Hospital.Services.PatientEr.IAllergyService, AllergyService>();
        services.AddScoped<Hospital.Shared.Services.IBillingService, BillingService>();
        services.AddScoped<Hospital.Services.PatientEr.IBillingService, BillingService>();
        services.AddScoped<Hospital.Shared.Services.IBloodCompatibilityService, BloodCompatibilityService>();
        services.AddScoped<Hospital.Services.PatientEr.IBloodCompatibilityService, BloodCompatibilityService>();
        services.AddScoped<Hospital.Services.PatientEr.IPrescriptionService, Hospital.Services.PatientEr.PrescriptionService>();
        services.AddScoped<Hospital.Shared.Services.IStatisticsService, StatisticsService>();
        services.AddScoped<Hospital.Services.PatientEr.IStatisticsService, StatisticsService>();
        services.AddScoped<Hospital.Shared.Services.ITransferLogService, TransferLogService>();
        services.AddScoped<Hospital.Services.PatientEr.ITransferLogService, TransferLogService>();
        services.AddScoped<Hospital.Shared.Services.ITransplantService, TransplantService>();
        services.AddScoped<Hospital.Services.PatientEr.ITransplantService, TransplantService>();
        services.AddScoped<Hospital.Shared.Services.IAddictDetectionService, AddictDetectionService>();
        services.AddScoped<Hospital.Services.PatientEr.IAddictDetectionService, AddictDetectionService>();
        services.AddScoped<Hospital.Shared.Services.IPatientService, PatientService>();
        services.AddScoped<Hospital.Shared.Services.IERRoomService, ERRoomService>();
        services.AddScoped<Hospital.Shared.Services.IERVisitService, ERVisitService>();
        services.AddScoped<Hospital.Shared.Services.ITriageService, TriageService>();
        services.AddScoped<Hospital.Shared.Services.ITriageParametersService, TriageParametersService>();
        services.AddScoped<Hospital.Shared.Services.ITriageDecisionService, TriageDecisionService>();
        services.AddScoped<Hospital.Shared.Services.IExaminationService, ExaminationService>();

#if false
        // Staff / Pharmacy domain (from 923-2) — stubs until Phase 3
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IBasketService, BasketService>();
        services.AddScoped<IUserAccountService, UserAccountService>();
        services.AddScoped<IShiftManagementService, ShiftManagementService>();
        services.AddScoped<IShiftSwapService, ShiftSwapService>();
        services.AddScoped<IDoctorAppointmentService, AppointmentService>();
        services.AddScoped<IMedicalEvaluationService, MedicalEvaluationService>();
        services.AddScoped<IERDispatchService, ERDispatchService>();
        services.AddScoped<IHangoutService, HangoutService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IPharmacyHandoverService, PharmacyHandoverService>();
#endif
        return services;
    }
}
