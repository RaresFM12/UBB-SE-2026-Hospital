using Hospital.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data;

public class HospitalDbContext(DbContextOptions<HospitalDbContext> options) : DbContext(options)
{
    // Users & Auth
    public DbSet<User> Users => Set<User>();
    public DbSet<UserDiscount> UserDiscounts => Set<UserDiscount>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<PeriodNote> PeriodNotes => Set<PeriodNote>();
    public DbSet<BasketEntry> BasketEntries => Set<BasketEntry>();

    // Staff
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<ShiftSwapRequest> ShiftSwapRequests => Set<ShiftSwapRequest>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MedicalEvaluation> MedicalEvaluations => Set<MedicalEvaluation>();
    public DbSet<ERRequest> ERRequests => Set<ERRequest>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Hangout> Hangouts => Set<Hangout>();
    public DbSet<HangoutParticipant> HangoutParticipants => Set<HangoutParticipant>();
    public DbSet<PharmacyHandover> PharmacyHandovers => Set<PharmacyHandover>();
    public DbSet<HighRiskMedicine> HighRiskMedicines => Set<HighRiskMedicine>();

    // Pharmacy / Inventory
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemBatch> ItemBatches => Set<ItemBatch>();
    public DbSet<ItemSubstance> ItemSubstances => Set<ItemSubstance>();
    public DbSet<Substance> Substances => Set<Substance>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    // Patients
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Allergy> Allergies => Set<Allergy>();
    public DbSet<PatientAllergy> PatientAllergies => Set<PatientAllergy>();
    public DbSet<MedicalHistory> MedicalHistories => Set<MedicalHistory>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<Transplant> Transplants => Set<Transplant>();
    public DbSet<TransplantMatch> TransplantMatches => Set<TransplantMatch>();

    // ER
    public DbSet<ERVisit> ERVisits => Set<ERVisit>();
    public DbSet<ERRoom> ERRooms => Set<ERRoom>();
    public DbSet<Triage> Triages => Set<Triage>();
    public DbSet<TriageParameters> TriageParameters => Set<TriageParameters>();
    public DbSet<Examination> Examinations => Set<Examination>();
    public DbSet<TransferLog> TransferLogs => Set<TransferLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

        // TPH for Staff hierarchy
        modelBuilder.Entity<Staff>().HasDiscriminator<string>("Role")
            .HasValue<Staff>("Staff")
            .HasValue<Doctor>("Doctor")
            .HasValue<Pharmacyst>("Pharmacist");

        modelBuilder.Entity<PatientAllergy>()
            .HasKey(pa => new { pa.MedicalHistoryId, pa.AllergyId });
    }
}
