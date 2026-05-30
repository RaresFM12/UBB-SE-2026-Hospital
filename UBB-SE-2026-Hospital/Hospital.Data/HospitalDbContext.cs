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

        // Non-standard primary keys
        modelBuilder.Entity<Staff>().HasKey(s => s.StaffID);
        modelBuilder.Entity<ERRoom>().HasKey(r => r.RoomId);
        modelBuilder.Entity<ERVisit>().HasKey(v => v.VisitId);
        modelBuilder.Entity<ShiftSwapRequest>().HasKey(s => s.SwapId);
        modelBuilder.Entity<MedicalEvaluation>().HasKey(e => e.EvaluationID);
        modelBuilder.Entity<Hangout>().HasKey(h => h.HangoutID);

        // TPH for Staff hierarchy
        modelBuilder.Entity<Staff>().HasDiscriminator<string>("Role")
            .HasValue<Staff>("Staff")
            .HasValue<Doctor>("Doctor")
            .HasValue<Pharmacyst>("Pharmacist");

        modelBuilder.Entity<PatientAllergy>()
            .HasKey(pa => new { pa.MedicalHistoryId, pa.AllergyId });

        // Explicitly map the two Staff↔ShiftSwapRequest relationships
        modelBuilder.Entity<ShiftSwapRequest>()
            .HasOne(s => s.Requester)
            .WithMany(st => st.ShiftSwapRequestsAsRequester)
            .HasForeignKey(s => s.RequestingStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ShiftSwapRequest>()
            .HasOne(s => s.Colleague)
            .WithMany(st => st.ShiftSwapRequestsAsColleague)
            .HasForeignKey(s => s.TargetStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        // Shift → Staff
        modelBuilder.Entity<Shift>()
            .HasOne(s => s.Staff)
            .WithMany(st => st.Shifts)
            .HasForeignKey(s => s.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        // Notification → Staff (via Recipient nav, StaffId FK)
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Recipient)
            .WithMany(st => st.Notifications)
            .HasForeignKey(n => n.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        // HangoutParticipant → Hangout / Staff
        modelBuilder.Entity<HangoutParticipant>()
            .HasOne(hp => hp.Hangout)
            .WithMany(h => h.HangoutParticipantEntries)
            .HasForeignKey(hp => hp.HangoutId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HangoutParticipant>()
            .HasOne(hp => hp.Staff)
            .WithMany(st => st.HangoutParticipantEntries)
            .HasForeignKey(hp => hp.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prescription is the dependent side of the one-to-one with MedicalRecord
        modelBuilder.Entity<Prescription>()
            .HasOne(p => p.MedicalRecord)
            .WithOne(r => r.Prescription)
            .HasForeignKey<Prescription>(p => p.RecordId)
            .OnDelete(DeleteBehavior.Cascade);

        // MedicalHistory stores ChronicConditions as JSON
        modelBuilder.Entity<MedicalHistory>()
            .Property(m => m.ChronicConditions)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null)!);

        // Item dictionaries are not mapped to DB columns
        modelBuilder.Entity<Item>()
            .Ignore(i => i.ActiveSubstances)
            .Ignore(i => i.Batches);

        // Order dictionary is not mapped
        modelBuilder.Entity<Order>()
            .Ignore(o => o.ItemQuantitiesWithFinalPrice);

        // User computed/notmapped collections
        modelBuilder.Entity<User>()
            .Ignore(u => u.PeriodNotes)
            .Ignore(u => u.StockAlerts)
            .Ignore(u => u.FavoriteItems)
            .Ignore(u => u.UserDiscounts)
            .Ignore(u => u.Basket);
    }
}
