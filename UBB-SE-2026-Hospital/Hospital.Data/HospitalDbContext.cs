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
        modelBuilder.Entity<Staff>().HasKey(s => s.StaffId);
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

        // ShiftSwapRequest → Staff (Requester / Colleague)
        modelBuilder.Entity<ShiftSwapRequest>()
            .HasOne(s => s.Requester)
            .WithMany(st => st.ShiftSwapRequestsAsRequester)
            .HasForeignKey("RequestingStaffId")
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ShiftSwapRequest>()
            .HasOne(s => s.Colleague)
            .WithMany(st => st.ShiftSwapRequestsAsColleague)
            .HasForeignKey("TargetStaffId")
            .OnDelete(DeleteBehavior.Restrict);

        // ShiftSwapRequest → Shift
        modelBuilder.Entity<ShiftSwapRequest>()
            .HasOne(s => s.Shift)
            .WithMany()
            .HasForeignKey("ShiftId")
            .OnDelete(DeleteBehavior.Restrict);

        // Shift → Staff
        modelBuilder.Entity<Shift>()
            .HasOne(s => s.Staff)
            .WithMany(st => st.Shifts)
            .HasForeignKey("StaffId")
            .OnDelete(DeleteBehavior.Cascade);

        // Notification → Staff
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Recipient)
            .WithMany(st => st.Notifications)
            .HasForeignKey("StaffId")
            .OnDelete(DeleteBehavior.Cascade);

        // HangoutParticipant → Hangout / Staff
        modelBuilder.Entity<HangoutParticipant>()
            .HasOne(hp => hp.Hangout)
            .WithMany(h => h.HangoutParticipantEntries)
            .HasForeignKey("HangoutId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HangoutParticipant>()
            .HasOne(hp => hp.Staff)
            .WithMany(st => st.HangoutParticipantEntries)
            .HasForeignKey("StaffId")
            .OnDelete(DeleteBehavior.Restrict);

        // Hangout → Staff (Organizer)
        modelBuilder.Entity<Hangout>()
            .HasOne(h => h.Organizer)
            .WithMany()
            .HasForeignKey("OrganizerId")
            .OnDelete(DeleteBehavior.Restrict);

        // Appointment → Doctor
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Doctor)
            .WithMany()
            .HasForeignKey("DoctorId")
            .OnDelete(DeleteBehavior.Restrict);

        // ERRequest → AssignedDoctor
        modelBuilder.Entity<ERRequest>()
            .HasOne(r => r.AssignedDoctor)
            .WithMany()
            .HasForeignKey("AssignedDoctorId")
            .OnDelete(DeleteBehavior.Restrict);

        // PharmacyHandover → Staff
        modelBuilder.Entity<PharmacyHandover>()
            .HasOne(h => h.Pharmacist)
            .WithMany()
            .HasForeignKey("PharmacistId")
            .OnDelete(DeleteBehavior.Restrict);

        // MedicalEvaluation → Doctor (Evaluator)
        modelBuilder.Entity<MedicalEvaluation>()
            .HasOne(e => e.Evaluator)
            .WithMany()
            .HasForeignKey("EvaluatorId")
            .OnDelete(DeleteBehavior.Restrict);

        // Prescription → MedicalRecord (one-to-one, dependent side)
        modelBuilder.Entity<Prescription>()
            .HasOne(p => p.MedicalRecord)
            .WithOne(r => r.Prescription)
            .HasForeignKey<Prescription>("RecordId")
            .OnDelete(DeleteBehavior.Cascade);

        // PrescriptionItem → Prescription
        modelBuilder.Entity<PrescriptionItem>()
            .HasOne(pi => pi.Prescription)
            .WithMany(p => p.MedicationList)
            .HasForeignKey("PrescriptionId")
            .OnDelete(DeleteBehavior.Cascade);

        // MedicalHistory stores ChronicConditions as JSON
        modelBuilder.Entity<MedicalHistory>()
            .Property(m => m.ChronicConditions)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null)!);

        // MedicalHistory → Patient
        modelBuilder.Entity<MedicalHistory>()
            .HasOne(m => m.Patient)
            .WithOne(p => p.MedicalHistory)
            .HasForeignKey<MedicalHistory>("PatientId")
            .OnDelete(DeleteBehavior.Restrict);

        // PatientAllergy → MedicalHistory / Allergy
        modelBuilder.Entity<PatientAllergy>()
            .HasOne(pa => pa.MedicalHistory)
            .WithMany(h => h.PatientAllergies)
            .HasForeignKey(pa => pa.MedicalHistoryId);

        modelBuilder.Entity<PatientAllergy>()
            .HasOne(pa => pa.Allergy)
            .WithMany()
            .HasForeignKey(pa => pa.AllergyId);

        // MedicalRecord → MedicalHistory
        modelBuilder.Entity<MedicalRecord>()
            .HasOne(r => r.MedicalHistory)
            .WithMany(h => h.MedicalRecords)
            .HasForeignKey("MedicalHistoryId")
            .OnDelete(DeleteBehavior.Restrict);

        // MedicalRecord → Staff / Transplant
        modelBuilder.Entity<MedicalRecord>()
            .HasOne(r => r.StaffMember)
            .WithMany()
            .HasForeignKey("StaffId")
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MedicalRecord>()
            .HasOne(r => r.Transplant)
            .WithMany()
            .HasForeignKey("TransplantId")
            .OnDelete(DeleteBehavior.Restrict);

        // Transplant → Patient (Receiver / Donor)
        modelBuilder.Entity<Transplant>()
            .HasOne(t => t.Receiver)
            .WithMany()
            .HasForeignKey("ReceiverId")
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transplant>()
            .HasOne(t => t.Donor)
            .WithMany()
            .HasForeignKey("DonorId")
            .OnDelete(DeleteBehavior.Restrict);

        // TransplantMatch → Transplant / Patient
        modelBuilder.Entity<TransplantMatch>()
            .HasOne(tm => tm.Transplant)
            .WithMany()
            .HasForeignKey("TransplantId")
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TransplantMatch>()
            .HasOne(tm => tm.Receiver)
            .WithMany()
            .HasForeignKey("ReceiverId")
            .OnDelete(DeleteBehavior.Restrict);

        // ERVisit → Patient
        modelBuilder.Entity<ERVisit>()
            .HasOne(v => v.Patient)
            .WithMany()
            .HasForeignKey("PatientId")
            .OnDelete(DeleteBehavior.Restrict);

        // ERRoom → ERVisit (current visit, nullable)
        modelBuilder.Entity<ERRoom>()
            .HasOne(r => r.CurrentVisit)
            .WithMany()
            .HasForeignKey("CurrentVisitId")
            .OnDelete(DeleteBehavior.Restrict);

        // Triage → ERVisit
        modelBuilder.Entity<Triage>()
            .HasOne(t => t.Visit)
            .WithMany()
            .HasForeignKey("VisitId")
            .OnDelete(DeleteBehavior.Restrict);

        // TriageParameters → Triage (one-to-one, cascade delete)
        modelBuilder.Entity<TriageParameters>()
            .HasOne(tp => tp.Triage)
            .WithOne()
            .HasForeignKey<TriageParameters>("TriageId")
            .OnDelete(DeleteBehavior.Cascade);

        // Examination → ERVisit / Staff / ERRoom
        modelBuilder.Entity<Examination>()
            .HasOne(e => e.Visit)
            .WithMany()
            .HasForeignKey("VisitId")
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Examination>()
            .HasOne(e => e.Doctor)
            .WithMany()
            .HasForeignKey("DoctorId")
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Examination>()
            .HasOne(e => e.Room)
            .WithMany()
            .HasForeignKey("RoomId")
            .OnDelete(DeleteBehavior.Restrict);

        // TransferLog → ERVisit
        modelBuilder.Entity<TransferLog>()
            .HasOne(tl => tl.Visit)
            .WithMany()
            .HasForeignKey("VisitId")
            .OnDelete(DeleteBehavior.Restrict);

        // Order → User (Client)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Client)
            .WithMany(u => u.Orders)
            .HasForeignKey("ClientId")
            .OnDelete(DeleteBehavior.Restrict);

        // OrderItem → Order / Item
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItemEntries)
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Item)
            .WithMany()
            .HasForeignKey("ItemId")
            .OnDelete(DeleteBehavior.Restrict);

        // ItemBatch → Item
        modelBuilder.Entity<ItemBatch>()
            .HasOne(b => b.Item)
            .WithMany(i => i.ItemBatchEntries)
            .HasForeignKey("ItemId")
            .OnDelete(DeleteBehavior.Cascade);

        // ItemSubstance → Item / Substance
        modelBuilder.Entity<ItemSubstance>()
            .HasOne(s => s.Item)
            .WithMany(i => i.ItemSubstanceEntries)
            .HasForeignKey("ItemId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ItemSubstance>()
            .HasOne(s => s.Substance)
            .WithMany(s => s.ItemSubstanceEntries)
            .HasForeignKey("SubstanceId")
            .OnDelete(DeleteBehavior.Restrict);

        // BasketEntry → User / Item
        modelBuilder.Entity<BasketEntry>()
            .HasOne(b => b.User)
            .WithMany()
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BasketEntry>()
            .HasOne(b => b.Item)
            .WithMany()
            .HasForeignKey("ItemId")
            .OnDelete(DeleteBehavior.Cascade);

        // UserDiscount → User / Item
        modelBuilder.Entity<UserDiscount>()
            .HasOne(d => d.User)
            .WithMany(u => u.UserDiscountEntries)
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserDiscount>()
            .HasOne(d => d.Item)
            .WithMany()
            .HasForeignKey("ItemId")
            .OnDelete(DeleteBehavior.Restrict);

        // UserNotification → User / Item
        modelBuilder.Entity<UserNotification>()
            .HasOne(n => n.User)
            .WithMany(u => u.UserNotificationEntries)
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserNotification>()
            .HasOne(n => n.Item)
            .WithMany()
            .HasForeignKey("ItemId")
            .OnDelete(DeleteBehavior.Restrict);

        // PeriodNote → User
        modelBuilder.Entity<PeriodNote>()
            .HasOne(n => n.User)
            .WithMany(u => u.PeriodNoteEntries)
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        // Substance reference data
        modelBuilder.Entity<Substance>().HasData(
            new Substance { Id = 1, Name = "Ibuprofen",      LethalDose = 3200.00f, Description = "Anti-inflammatory pain reliever" },
            new Substance { Id = 2, Name = "Paracetamol",    LethalDose = 4000.00f, Description = "Pain reliever and fever reducer" },
            new Substance { Id = 3, Name = "Magnesium",      LethalDose = 2500.00f, Description = "Mineral supplement for muscle and nerve support" },
            new Substance { Id = 4, Name = "Vitamin C",      LethalDose = 2000.00f, Description = "Vitamin supplement for immune support" },
            new Substance { Id = 5, Name = "Cetirizine",     LethalDose = 500.00f,  Description = "Antihistamine for allergy relief" },
            new Substance { Id = 6, Name = "Iron",           LethalDose = 45.00f,   Description = "Mineral supplement used for iron deficiency" },
            new Substance { Id = 7, Name = "Calcium",        LethalDose = 2500.00f, Description = "Mineral supplement for bones and muscles" },
            new Substance { Id = 8, Name = "Omega 3",        LethalDose = 3000.00f, Description = "Fatty acid supplement for heart and brain health" },
            new Substance { Id = 9, Name = "Melatonin",      LethalDose = 10.00f,   Description = "Sleep support supplement" },
            new Substance { Id = 10, Name = "Probiotics",    LethalDose = 1000.00f, Description = "Digestive support supplement" },
            new Substance { Id = 11, Name = "Zinc",          LethalDose = 40.00f,   Description = "Mineral supplement for immunity" },
            new Substance { Id = 12, Name = "Loratadine",    LethalDose = 1000.00f, Description = "Non-drowsy antihistamine" },
            new Substance { Id = 13, Name = "Loperamide",    LethalDose = 60.00f,   Description = "Medication to decrease frequency of diarrhea" },
            new Substance { Id = 14, Name = "Simethicone",   LethalDose = 2000.00f, Description = "Anti-foaming agent to reduce bloating and gas" },
            new Substance { Id = 15, Name = "Diclofenac",    LethalDose = 1500.00f, Description = "Nonsteroidal anti-inflammatory drug (NSAID)" },
            new Substance { Id = 16, Name = "Dexpanthenol",  LethalDose = 5000.00f, Description = "Skin protectant and moisturizer" },
            new Substance { Id = 17, Name = "Vitamin D3",    LethalDose = 50.00f,   Description = "Essential vitamin for bone health and immunity" },
            new Substance { Id = 18, Name = "Xylometazoline",LethalDose = 10.00f,   Description = "Decongestant for nasal passages" },
            new Substance { Id = 19, Name = "Acetylcysteine",LethalDose = 3000.00f, Description = "Mucolytic agent to clear mucus" }
        );

        // HighRiskMedicine reference data
        modelBuilder.Entity<HighRiskMedicine>().HasData(
            new HighRiskMedicine { Id = 1, MedicineName = "Warfarin",      WarningMessage = "Anticoagulant - check INR before prescribing." },
            new HighRiskMedicine { Id = 2, MedicineName = "Methotrexate",  WarningMessage = "Hepatotoxic - confirm dosing and weekly schedule." }
        );

        // Allergy reference data
        modelBuilder.Entity<Allergy>().HasData(
            new Allergy { AllergyId = 1, AllergyName = "Penicillin",        AllergyType = "Drug",  AllergyCategory = "Antibiotic" },
            new Allergy { AllergyId = 2, AllergyName = "Peanuts",           AllergyType = "Food",  AllergyCategory = "Nut" },
            new Allergy { AllergyId = 3, AllergyName = "Latex",             AllergyType = "Contact",AllergyCategory = "Material" },
            new Allergy { AllergyId = 4, AllergyName = "Ibuprofen",         AllergyType = "Drug",  AllergyCategory = "NSAID" },
            new Allergy { AllergyId = 5, AllergyName = "Sulfonamides",      AllergyType = "Drug",  AllergyCategory = "Antibiotic" },
            new Allergy { AllergyId = 6, AllergyName = "Shellfish",         AllergyType = "Food",  AllergyCategory = "Seafood" },
            new Allergy { AllergyId = 7, AllergyName = "Pollen",            AllergyType = "Environmental", AllergyCategory = "Seasonal" },
            new Allergy { AllergyId = 8, AllergyName = "Dust Mites",        AllergyType = "Environmental", AllergyCategory = "Perennial" },
            new Allergy { AllergyId = 9, AllergyName = "Aspirin",           AllergyType = "Drug",  AllergyCategory = "Salicylate" },
            new Allergy { AllergyId = 10,AllergyName = "Dairy",             AllergyType = "Food",  AllergyCategory = "Lactose" }
        );

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
