using Hospital.Data.Repositories;
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

        // Staff & Pharmacy
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<IShiftSwapRepository, ShiftSwapRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IEvaluationsRepository, EvaluationsRepository>();
        services.AddScoped<IERDispatchRepository, ERDispatchRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IHangoutRepository, HangoutRepository>();
        services.AddScoped<IHangoutParticipantRepository, HangoutParticipantRepository>();
        services.AddScoped<IPharmacyHandoverRepository, PharmacyHandoverRepository>();
        services.AddScoped<IHighRiskMedicineRepository, HighRiskMedicineRepository>();
        services.AddScoped<IItemsRepository, ItemsRepository>();
        services.AddScoped<IOrdersRepository, OrdersRepository>();
        services.AddScoped<ISubstancesRepository, SubstancesRepository>();
        services.AddScoped<IBasketRepository, BasketRepository>();

        // Patient & ER
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IAllergyRepository, AllergyRepository>();
        services.AddScoped<IMedicalHistoryRepository, MedicalHistoryRepository>();
        services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
        services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
        services.AddScoped<ITransplantRepository, TransplantRepository>();
        services.AddScoped<ITransferLogRepository, TransferLogRepository>();
        services.AddScoped<ITriageRepository, TriageRepository>();
        services.AddScoped<ITriageParametersRepository, TriageParametersRepository>();
        services.AddScoped<IExaminationRepository, ExaminationRepository>();
        services.AddScoped<IERRoomRepository, ERRoomRepository>();
        services.AddScoped<IERVisitRepository, ERVisitRepository>();

        return services;
    }
}
