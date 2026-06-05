using Hospital.Data.Models;
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

    // Authorization (roles & modules)
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<RoleModulePermission> RoleModulePermissions => Set<RoleModulePermission>();

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

        modelBuilder.Entity<User>().HasIndex(user => user.Email).IsUnique();
        modelBuilder.Entity<User>().HasIndex(user => user.Username).IsUnique();

        ConfigureAuthorization(modelBuilder);
        modelBuilder.Entity<Shift>(entity =>
        {
            entity.Property(shift => shift.StartTime).HasColumnName("StartTime");
            entity.Property(shift => shift.EndTime).HasColumnName("EndTime");
        });
        // Non-standard primary keys
        modelBuilder.Entity<Staff>().HasKey(staff => staff.StaffId);
        modelBuilder.Entity<ERRoom>().HasKey(room => room.RoomId);
        modelBuilder.Entity<ERVisit>().HasKey(visit => visit.VisitId);
        modelBuilder.Entity<ShiftSwapRequest>().HasKey(swapRequest => swapRequest.SwapId);
        modelBuilder.Entity<MedicalEvaluation>().HasKey(evaluation => evaluation.EvaluationID);
        modelBuilder.Entity<Hangout>().HasKey(hangout => hangout.HangoutID);

        // TPH for Staff hierarchy
        modelBuilder.Entity<Staff>().HasDiscriminator<string>("Role")
            .HasValue<Staff>("Staff")
            .HasValue<Doctor>("Doctor")
            .HasValue<Pharmacyst>("Pharmacist");

        modelBuilder.Entity<PatientAllergy>()
            .HasKey(patientAllergy => new { patientAllergy.MedicalHistoryId, patientAllergy.AllergyId });

        // ShiftSwapRequest → Staff (Requester / Colleague)
        modelBuilder.Entity<ShiftSwapRequest>()
            .HasOne(swapRequest => swapRequest.Requester)
            .WithMany(staff => staff.ShiftSwapRequestsAsRequester)
            .HasForeignKey("RequestingStaffId")
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ShiftSwapRequest>()
            .HasOne(swapRequest => swapRequest.Colleague)
            .WithMany(staff => staff.ShiftSwapRequestsAsColleague)
            .HasForeignKey("TargetStaffId")
            .OnDelete(DeleteBehavior.Restrict);

        // ShiftSwapRequest → Shift
        modelBuilder.Entity<ShiftSwapRequest>()
            .HasOne(swapRequest => swapRequest.Shift)
            .WithMany()
            .HasForeignKey("ShiftId")
            .OnDelete(DeleteBehavior.Restrict);

        // Shift → Staff
        modelBuilder.Entity<Shift>()
            .HasOne(shift => shift.Staff)
            .WithMany(staff => staff.Shifts)
            .HasForeignKey("StaffId")
            .OnDelete(DeleteBehavior.Cascade);

        // Notification → Staff
        modelBuilder.Entity<Notification>()
            .HasOne(notification => notification.Recipient)
            .WithMany(staff => staff.Notifications)
            .HasForeignKey("StaffId")
            .OnDelete(DeleteBehavior.Cascade);

        // HangoutParticipant → Hangout / Staff
        modelBuilder.Entity<HangoutParticipant>()
            .HasOne(participant => participant.Hangout)
            .WithMany(hangout => hangout.HangoutParticipantEntries)
            .HasForeignKey("HangoutId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HangoutParticipant>()
            .HasOne(participant => participant.Staff)
            .WithMany(staff => staff.HangoutParticipantEntries)
            .HasForeignKey("StaffId")
            .OnDelete(DeleteBehavior.Restrict);

        // Hangout → Staff (Organizer)
        modelBuilder.Entity<Hangout>()
            .HasOne(hangout => hangout.Organizer)
            .WithMany()
            .HasForeignKey("OrganizerId")
            .OnDelete(DeleteBehavior.Restrict);

        // Appointment → Doctor
        modelBuilder.Entity<Appointment>()
            .HasOne(appointment => appointment.Doctor)
            .WithMany()
            .HasForeignKey("DoctorId")
            .OnDelete(DeleteBehavior.Restrict);

        // ERRequest → AssignedDoctor
        modelBuilder.Entity<ERRequest>()
            .HasOne(request => request.AssignedDoctor)
            .WithMany()
            .HasForeignKey("AssignedDoctorId")
            .OnDelete(DeleteBehavior.Restrict);

        // PharmacyHandover → Staff
        modelBuilder.Entity<PharmacyHandover>()
            .HasOne(handover => handover.Pharmacist)
            .WithMany()
            .HasForeignKey("PharmacistId")
            .OnDelete(DeleteBehavior.Restrict);

        // MedicalEvaluation → Doctor (Evaluator)
        modelBuilder.Entity<MedicalEvaluation>()
            .HasOne(evaluation => evaluation.Evaluator)
            .WithMany()
            .HasForeignKey("EvaluatorId")
            .OnDelete(DeleteBehavior.Restrict);

        // Prescription → MedicalRecord (one-to-one, dependent side)
        modelBuilder.Entity<Prescription>()
            .HasOne(prescription => prescription.MedicalRecord)
            .WithOne(medicalRecord => medicalRecord.Prescription)
            .HasForeignKey<Prescription>("RecordId")
            .OnDelete(DeleteBehavior.Cascade);

        // PrescriptionItem → Prescription
        modelBuilder.Entity<PrescriptionItem>()
            .HasOne(prescriptionItem => prescriptionItem.Prescription)
            .WithMany(prescription => prescription.MedicationList)
            .HasForeignKey("PrescriptionId")
            .OnDelete(DeleteBehavior.Cascade);

        // MedicalHistory stores ChronicConditions as JSON
        modelBuilder.Entity<MedicalHistory>()
            .Property(medicalHistory => medicalHistory.ChronicConditions)
            .HasConversion(
                chronicConditions => System.Text.Json.JsonSerializer.Serialize(chronicConditions, (System.Text.Json.JsonSerializerOptions?)null),
                chronicConditionsJson => System.Text.Json.JsonSerializer.Deserialize<List<string>>(chronicConditionsJson, (System.Text.Json.JsonSerializerOptions?)null)!);

        // MedicalHistory → Patient
        modelBuilder.Entity<MedicalHistory>()
            .HasOne(medicalHistory => medicalHistory.Patient)
            .WithOne(patient => patient.MedicalHistory)
            .HasForeignKey<MedicalHistory>("PatientId")
            .OnDelete(DeleteBehavior.Restrict);

        // PatientAllergy → MedicalHistory / Allergy
        modelBuilder.Entity<PatientAllergy>()
            .HasOne(patientAllergy => patientAllergy.MedicalHistory)
            .WithMany(medicalHistory => medicalHistory.PatientAllergies)
            .HasForeignKey(patientAllergy => patientAllergy.MedicalHistoryId);

        modelBuilder.Entity<PatientAllergy>()
            .HasOne(patientAllergy => patientAllergy.Allergy)
            .WithMany()
            .HasForeignKey(patientAllergy => patientAllergy.AllergyId);

        // MedicalRecord → MedicalHistory
        modelBuilder.Entity<MedicalRecord>()
            .HasOne(medicalRecord => medicalRecord.MedicalHistory)
            .WithMany(medicalHistory => medicalHistory.MedicalRecords)
            .HasForeignKey("MedicalHistoryId")
            .OnDelete(DeleteBehavior.Restrict);

        // MedicalRecord → Staff / Transplant
        modelBuilder.Entity<MedicalRecord>()
            .HasOne(medicalRecord => medicalRecord.StaffMember)
            .WithMany()
            .HasForeignKey("StaffId")
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MedicalRecord>()
            .HasOne(medicalRecord => medicalRecord.Transplant)
            .WithMany()
            .HasForeignKey("TransplantId")
            .OnDelete(DeleteBehavior.Restrict);

        // Transplant → Patient (Receiver / Donor)
        modelBuilder.Entity<Transplant>()
            .HasOne(transplant => transplant.Receiver)
            .WithMany()
            .HasForeignKey("ReceiverId")
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transplant>()
            .HasOne(transplant => transplant.Donor)
            .WithMany()
            .HasForeignKey("DonorId")
            .OnDelete(DeleteBehavior.Restrict);

        // TransplantMatch → Transplant / Patient
        modelBuilder.Entity<TransplantMatch>()
            .HasOne(transplantMatch => transplantMatch.Transplant)
            .WithMany()
            .HasForeignKey("TransplantId")
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TransplantMatch>()
            .HasOne(transplantMatch => transplantMatch.Receiver)
            .WithMany()
            .HasForeignKey("ReceiverId")
            .OnDelete(DeleteBehavior.Restrict);

        // ERVisit → Patient
        modelBuilder.Entity<ERVisit>()
            .HasOne(visit => visit.Patient)
            .WithMany()
            .HasForeignKey("PatientId")
            .OnDelete(DeleteBehavior.Restrict);

        // ERRoom → ERVisit (current visit, nullable)
        modelBuilder.Entity<ERRoom>()
            .HasOne(room => room.CurrentVisit)
            .WithMany()
            .HasForeignKey("CurrentVisitId")
            .OnDelete(DeleteBehavior.Restrict);

        // Triage → ERVisit
        modelBuilder.Entity<Triage>()
            .HasOne(triage => triage.Visit)
            .WithMany()
            .HasForeignKey("VisitId")
            .OnDelete(DeleteBehavior.Restrict);

        // TriageParameters → Triage (one-to-one, cascade delete)
        modelBuilder.Entity<TriageParameters>()
            .HasOne(triageParameters => triageParameters.Triage)
            .WithOne()
            .HasForeignKey<TriageParameters>("TriageId")
            .OnDelete(DeleteBehavior.Cascade);

        // Examination → ERVisit / Staff / ERRoom
        modelBuilder.Entity<Examination>()
            .HasOne(examination => examination.Visit)
            .WithMany()
            .HasForeignKey("VisitId")
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Examination>()
            .HasOne(examination => examination.Doctor)
            .WithMany()
            .HasForeignKey("DoctorId")
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Examination>()
            .HasOne(examination => examination.Room)
            .WithMany()
            .HasForeignKey("RoomId")
            .OnDelete(DeleteBehavior.Restrict);

        // TransferLog → ERVisit
        modelBuilder.Entity<TransferLog>()
            .HasOne(transferLog => transferLog.Visit)
            .WithMany()
            .HasForeignKey("VisitId")
            .OnDelete(DeleteBehavior.Restrict);

        // Order → User (Client)
        modelBuilder.Entity<Order>()
            .HasOne(order => order.Client)
            .WithMany(user => user.Orders)
            .HasForeignKey("ClientId")
            .OnDelete(DeleteBehavior.Restrict);

        // OrderItem → Order / Item
        modelBuilder.Entity<OrderItem>()
            .HasOne(orderItem => orderItem.Order)
            .WithMany(order => order.OrderItemEntries)
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(orderItem => orderItem.Item)
            .WithMany()
            .HasForeignKey("ItemId")
            .OnDelete(DeleteBehavior.Restrict);

        // ItemBatch → Item
        modelBuilder.Entity<ItemBatch>()
            .HasOne(itemBatch => itemBatch.Item)
            .WithMany(item => item.ItemBatchEntries)
            .HasForeignKey("ItemId")
            .OnDelete(DeleteBehavior.Cascade);

        // ItemSubstance → Item / Substance
        modelBuilder.Entity<ItemSubstance>()
            .HasOne(itemSubstance => itemSubstance.Item)
            .WithMany(item => item.ItemSubstanceEntries)
            .HasForeignKey("ItemId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ItemSubstance>()
            .HasOne(itemSubstance => itemSubstance.Substance)
            .WithMany(substance => substance.ItemSubstanceEntries)
            .HasForeignKey("SubstanceId")
            .OnDelete(DeleteBehavior.Restrict);

        // BasketEntry → User / Item
        modelBuilder.Entity<BasketEntry>()
            .HasOne(basketEntry => basketEntry.User)
            .WithMany()
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BasketEntry>()
            .HasOne(basketEntry => basketEntry.Item)
            .WithMany()
            .HasForeignKey("ItemId")
            .OnDelete(DeleteBehavior.Cascade);

        // UserDiscount → User / Item
        modelBuilder.Entity<UserDiscount>()
            .HasOne(userDiscount => userDiscount.User)
            .WithMany(user => user.UserDiscountEntries)
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserDiscount>()
            .HasOne(userDiscount => userDiscount.Item)
            .WithMany()
            .HasForeignKey("ItemId")
            .OnDelete(DeleteBehavior.Restrict);

        // UserNotification → User / Item
        modelBuilder.Entity<UserNotification>()
            .HasOne(userNotification => userNotification.User)
            .WithMany(user => user.UserNotificationEntries)
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserNotification>()
            .HasOne(userNotification => userNotification.Item)
            .WithMany()
            .HasForeignKey("ItemId")
            .OnDelete(DeleteBehavior.Restrict);

        // PeriodNote → User
        modelBuilder.Entity<PeriodNote>()
            .HasOne(periodNote => periodNote.User)
            .WithMany(user => user.PeriodNoteEntries)
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
            .Ignore(item => item.ActiveSubstances)
            .Ignore(item => item.Batches);

        // Order dictionary is not mapped
        modelBuilder.Entity<Order>()
            .Ignore(order => order.ItemQuantitiesWithFinalPrice);

        // User computed/notmapped collections
        modelBuilder.Entity<User>()
            .Ignore(user => user.PeriodNotes)
            .Ignore(user => user.StockAlerts)
            .Ignore(user => user.FavoriteItems)
            .Ignore(user => user.UserDiscounts)
            .Ignore(user => user.Basket);
    }

    private static void ConfigureAuthorization(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasIndex(role => role.Name).IsUnique();
        modelBuilder.Entity<Module>().HasIndex(module => module.Key).IsUnique();

        modelBuilder.Entity<RoleModulePermission>()
            .HasKey(permission => new { permission.RoleId, permission.ModuleId });

        modelBuilder.Entity<RoleModulePermission>()
            .HasOne(permission => permission.Role)
            .WithMany(role => role.ModulePermissions)
            .HasForeignKey(permission => permission.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RoleModulePermission>()
            .HasOne(permission => permission.Module)
            .WithMany(module => module.RolePermissions)
            .HasForeignKey(permission => permission.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin",         Description = "Full system administrator" },
            new Role { Id = 2, Name = "Doctor",        Description = "Attending physician" },
            new Role { Id = 3, Name = "Pharmacist",    Description = "Pharmacy staff" },
            new Role { Id = 4, Name = "Nurse",         Description = "Nursing staff" },
            new Role { Id = 5, Name = "Client",        Description = "Pharmacy customer" },
            new Role { Id = 6, Name = "Patient",       Description = "Registered patient" },
            new Role { Id = 7, Name = "ERDoctor",      Description = "Emergency room physician" },
            new Role { Id = 8, Name = "LabTechnician", Description = "Laboratory technician" }
        );

        modelBuilder.Entity<Module>().HasData(
            new Module { Id = 1,  Key = "statistics",           Name = "Statistics",           Description = "Reporting and statistics dashboards" },
            new Module { Id = 2,  Key = "pharmacy",             Name = "Pharmacy",             Description = "Pharmacy catalogue and inventory" },
            new Module { Id = 3,  Key = "patient-registration", Name = "Patient Registration", Description = "Register and manage patients" },
            new Module { Id = 4,  Key = "queue",                Name = "Queue",                Description = "ER patient queue" },
            new Module { Id = 5,  Key = "triage",               Name = "Triage",               Description = "Triage assessment" },
            new Module { Id = 6,  Key = "room-assignment",      Name = "Room Assignment",      Description = "Assign patients to ER rooms" },
            new Module { Id = 7,  Key = "examination",          Name = "Examination",          Description = "Patient examinations" },
            new Module { Id = 8,  Key = "transfer-log",         Name = "Transfer Log",         Description = "Patient transfer records" },
            new Module { Id = 9,  Key = "room-management",      Name = "Room Management",      Description = "Manage ER rooms" },
            new Module { Id = 10, Key = "users",                Name = "Users",                Description = "User administration" },
            new Module { Id = 11, Key = "appointments",         Name = "Appointments",         Description = "Doctor appointments" },
            new Module { Id = 12, Key = "orders",               Name = "Orders",               Description = "Pharmacy orders and basket" },
            new Module { Id = 13, Key = "prescriptions",        Name = "Prescriptions",        Description = "Medical prescriptions" },
            new Module { Id = 14, Key = "shifts",               Name = "Shifts",               Description = "Staff shifts and swaps" },
            new Module { Id = 15, Key = "hangouts",             Name = "Hangouts",             Description = "Staff social hangouts" },
            new Module { Id = 16, Key = "billing",              Name = "Billing",              Description = "Billing and invoicing" }
        );

        modelBuilder.Entity<RoleModulePermission>().HasData(
            // Admin -> everything
            Permission(1, 1), Permission(1, 2), Permission(1, 3), Permission(1, 4),
            Permission(1, 5), Permission(1, 6), Permission(1, 7), Permission(1, 8),
            Permission(1, 9), Permission(1, 10), Permission(1, 11), Permission(1, 12),
            Permission(1, 13), Permission(1, 14), Permission(1, 15), Permission(1, 16),
            // Doctor
            Permission(2, 1), Permission(2, 3), Permission(2, 4), Permission(2, 5),
            Permission(2, 6), Permission(2, 7), Permission(2, 8), Permission(2, 11),
            Permission(2, 13),
            // Pharmacist
            Permission(3, 2), Permission(3, 12), Permission(3, 13), Permission(3, 16),
            // Nurse
            Permission(4, 3), Permission(4, 4), Permission(4, 5), Permission(4, 6),
            Permission(4, 7), Permission(4, 8),
            // Client
            Permission(5, 2), Permission(5, 12),
            // Patient
            Permission(6, 11),
            // ERDoctor
            Permission(7, 3), Permission(7, 4), Permission(7, 5), Permission(7, 6),
            Permission(7, 7), Permission(7, 8), Permission(7, 9),
            // LabTechnician
            Permission(8, 7)
        );
    }

    private static RoleModulePermission Permission(int roleId, int moduleId)
        => new() { RoleId = roleId, ModuleId = moduleId };
}
