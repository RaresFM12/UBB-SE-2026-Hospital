using Hospital.Shared.Services;

namespace Hospital.Desktop.Services;

public static class ServiceWrapper
{
    public static IUserAccountService UserAccountService => App.Services.GetRequiredService<IUserAccountService>();
    public static IAdminService AdminService => App.Services.GetRequiredService<IAdminService>();
    public static IOrderService OrderService => App.Services.GetRequiredService<IOrderService>();
    public static IShiftManagementService ShiftManagementService => App.Services.GetRequiredService<IShiftManagementService>();
    public static IDoctorAppointmentService DoctorAppointmentService => App.Services.GetRequiredService<IDoctorAppointmentService>();
    public static IERDispatchService ERDispatchService => App.Services.GetRequiredService<IERDispatchService>();
    public static IFatigueAuditService FatigueAuditService => App.Services.GetRequiredService<IFatigueAuditService>();
    public static IPatientService PatientService => App.Services.GetRequiredService<IPatientService>();
    public static IERRoomService ERRoomService => App.Services.GetRequiredService<IERRoomService>();
    public static IERVisitService ERVisitService => App.Services.GetRequiredService<IERVisitService>();
    public static ITriageService TriageService => App.Services.GetRequiredService<ITriageService>();
    public static IExaminationService ExaminationService => App.Services.GetRequiredService<IExaminationService>();
    public static ITransferLogService TransferLogService => App.Services.GetRequiredService<ITransferLogService>();
    public static IStatisticsService StatisticsService => App.Services.GetRequiredService<IStatisticsService>();
    public static IBillingService BillingService => App.Services.GetRequiredService<IBillingService>();
    public static ITransplantService TransplantService => App.Services.GetRequiredService<ITransplantService>();
    public static IBloodCompatibilityService BloodCompatibilityService => App.Services.GetRequiredService<IBloodCompatibilityService>();
    public static IAllergyService AllergyService => App.Services.GetRequiredService<IAllergyService>();
    public static IAddictDetectionService AddictDetectionService => App.Services.GetRequiredService<IAddictDetectionService>();
    public static IPrescriptionService PrescriptionService => App.Services.GetRequiredService<IPrescriptionService>();
}
