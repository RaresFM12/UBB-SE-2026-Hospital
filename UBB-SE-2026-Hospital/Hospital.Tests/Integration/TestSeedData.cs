using Hospital.Data;
using Hospital.Data.Models;

namespace Hospital.Tests.Integration;

/// <summary>
/// Identifiers and credentials assigned while seeding so tests can target the
/// exact rows that were created.
/// </summary>
public sealed class SeededIds
{
    public const string AdminEmail = "admin@hospital.test";
    public const string DoctorEmail = "doctor@hospital.test";
    public const string NurseEmail = "nurse@hospital.test";
    public const string PharmacistEmail = "pharmacist@hospital.test";
    public const string ClientEmail = "client@hospital.test";
    public const string DisabledEmail = "disabled@hospital.test";

    // Passwords intentionally contain '#', which is not a valid Base64 char, so
    // AuthService.VerifyPassword takes the plaintext-comparison fallback branch.
    public const string Password = "P@ssw0rd#Test";

    public int AdminUserId { get; set; }
    public int DoctorUserId { get; set; }
    public int NurseUserId { get; set; }
    public int PharmacistUserId { get; set; }
    public int ClientUserId { get; set; }
    public int DisabledUserId { get; set; }

    public int ActivePatientId { get; set; }
    public int ArchivedPatientId { get; set; }

    public int DoctorStaffId { get; set; }
    public int PharmacistStaffId { get; set; }

    public int ShiftId { get; set; }
    public int OrderId { get; set; }
    public int ItemId { get; set; }
}

/// <summary>
/// Inserts a deterministic fixture into a fresh in-memory database.
/// </summary>
public static class TestSeedData
{
    public static SeededIds Seed(HospitalDbContext context)
    {
        var ids = new SeededIds();

        var admin = NewUser(SeededIds.AdminEmail, "admin", "Admin", isAdmin: true);
        var doctorUser = NewUser(SeededIds.DoctorEmail, "doctor", "Doctor");
        var nurseUser = NewUser(SeededIds.NurseEmail, "nurse", "Nurse");
        var pharmacistUser = NewUser(SeededIds.PharmacistEmail, "pharma", "Pharmacist");
        var clientUser = NewUser(SeededIds.ClientEmail, "client", "Client");
        var disabledUser = NewUser(SeededIds.DisabledEmail, "disabled", "Admin", isDisabled: true);

        context.Users.AddRange(admin, doctorUser, nurseUser, pharmacistUser, clientUser, disabledUser);
        context.SaveChanges();

        ids.AdminUserId = admin.Id;
        ids.DoctorUserId = doctorUser.Id;
        ids.NurseUserId = nurseUser.Id;
        ids.PharmacistUserId = pharmacistUser.Id;
        ids.ClientUserId = clientUser.Id;
        ids.DisabledUserId = disabledUser.Id;

        var activePatient = new Patient
        {
            FirstName = "Ana",
            LastName = "Pop",
            Cnp = "2940101123456",
            DateOfBirth = new DateTime(1994, 1, 1),
            Sex = Sex.F,
            PhoneNumber = "0700000001",
            EmergencyContact = "Ion Pop 0700000002",
            IsArchived = false,
            IsDonor = true,
            MedicalHistory = new MedicalHistory
            {
                BloodType = BloodType.O,
                Rh = Rh.Positive,
                ChronicConditions = new List<string> { "Asthma" },
            },
        };

        var archivedPatient = new Patient
        {
            FirstName = "Vlad",
            LastName = "Ionescu",
            Cnp = "1700101123456",
            DateOfBirth = new DateTime(1970, 1, 1),
            Sex = Sex.M,
            PhoneNumber = "0700000003",
            EmergencyContact = "Maria Ionescu 0700000004",
            IsArchived = true,
            IsDonor = false,
            MedicalHistory = new MedicalHistory
            {
                BloodType = BloodType.A,
                Rh = Rh.Negative,
            },
        };

        context.Patients.AddRange(activePatient, archivedPatient);
        context.SaveChanges();
        ids.ActivePatientId = activePatient.PatientId;
        ids.ArchivedPatientId = archivedPatient.PatientId;

        var doctorStaff = new Doctor
        {
            Email = "dr.house@hospital.test",
            FirstName = "Gregory",
            LastName = "House",
            Department = "Diagnostics",
            Status = "Available",
            Available = true,
            HourlyRate = 120,
            DoctorStatus = DoctorStatus.Available,
        };

        var pharmacistStaff = new Pharmacyst
        {
            Email = "pharma.staff@hospital.test",
            FirstName = "Pia",
            LastName = "Mortar",
            Department = "Pharmacy",
            Status = "Available",
            Available = true,
            HourlyRate = 80,
        };

        context.Staff.AddRange(doctorStaff, pharmacistStaff);
        context.SaveChanges();
        ids.DoctorStaffId = doctorStaff.StaffId;
        ids.PharmacistStaffId = pharmacistStaff.StaffId;

        var shift = new Shift(0, doctorStaff, "Diagnostics", new DateTime(2025, 1, 6, 8, 0, 0), new DateTime(2025, 1, 6, 16, 0, 0), ShiftStatus.Scheduled);
        context.Shifts.Add(shift);
        context.SaveChanges();
        ids.ShiftId = shift.Id;

        var item = new Item("Aspirin", "Bayer", "Painkiller", price: 12.5f, numberOfPills: 20, quantity: 100);
        context.Items.Add(item);
        context.SaveChanges();
        ids.ItemId = item.Id;

        var order = new Order(0, clientUser, new DateOnly(2025, 1, 10));
        context.Orders.Add(order);
        context.SaveChanges();
        ids.OrderId = order.Id;

        return ids;
    }

    private static User NewUser(string email, string username, string role, bool isAdmin = false, bool isDisabled = false)
        => new()
        {
            Email = email,
            Username = username,
            PhoneNumber = "0711111111",
            PasswordHash = SeededIds.Password,
            Role = role,
            IsAdmin = isAdmin,
            IsDisabled = isDisabled,
        };
}
