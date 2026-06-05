using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Hospital.Services.Auth;
using Hospital.Services.PatientEr;
using Hospital.Shared.DTOs.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Hospital.Tests.Integration
{
    // Automated versions of the documented manual QA test cases in docs/manual-tests/.
    // Each test mirrors a manual scenario end-to-end against the real service +
    // repository + in-memory EF Core stack.
    //
    // Mapping:
    //   01-Authentication_Login_Success      -> Login_WithValidCredentials_*
    //   02-Authentication_Login_DisabledUser -> Login_ForDisabledUser_*
    //   03-ERVisit_CreateAndComplete         -> ERVisit_CreateTriageAndComplete_*
    //   08-Patient_Profile_EditValidate      -> Patient_EditProfile_*
    //   11-Transplant_RegisterDonorRecipient -> Transplant_RegisterDonorAndRecipient_*
    //   12-Billing_ApplyAndPersistDiscount   -> Billing_ApplyAndPersistDiscount_*
    [TestClass]
    public sealed class ManualScenarioIntegrationTests
    {
        private static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password), salt, 100_000, HashAlgorithmName.SHA256, 32);
            byte[] combined = new byte[48];
            Buffer.BlockCopy(salt, 0, combined, 0, 16);
            Buffer.BlockCopy(hash, 0, combined, 16, 32);
            return Convert.ToBase64String(combined);
        }

        private static IConfiguration BuildConfiguration()
        {
            var config = new Mock<IConfiguration>();
            config.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("60");
            config.Setup(c => c["Jwt:Key"]).Returns("super-secret-signing-key-with-enough-length-1234567890");
            config.Setup(c => c["Jwt:Issuer"]).Returns("HospitalTests");
            config.Setup(c => c["Jwt:Audience"]).Returns("HospitalTests");
            return config.Object;
        }

        private static async Task SeedUserAsync(
            Hospital.Data.HospitalDbContext context, string email, string password, bool disabled = false)
        {
            context.Users.Add(new User
            {
                Email = email,
                Username = "test.user",
                PasswordHash = HashPassword(password),
                Role = "Doctor",
                IsDisabled = disabled,
            });
            await context.SaveChangesAsync();
        }

        private static Patient NewPatient(string first = "Test", string cnp = "1234567890123", Sex sex = Sex.F)
            => new Patient
            {
                FirstName = first, LastName = "User", Cnp = cnp,
                DateOfBirth = new DateTime(1990, 1, 1), Sex = sex,
                PhoneNumber = "0700000000", EmergencyContact = "Contact",
            };

        // 01 - Authentication: Login Success
        [TestMethod]
        public async Task Login_WithValidCredentials_IssuesToken()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await SeedUserAsync(context, "test.user@hospital.local", "Password123!");
            var service = new AuthService(new UsersRepository(context), BuildConfiguration());

            AuthResponse response = await service.LoginAsync(
                new LoginRequest { Email = "test.user@hospital.local", Password = "Password123!" });

            Assert.IsFalse(string.IsNullOrWhiteSpace(response.Token));
            Assert.IsTrue(response.ExpiresAtUtc > DateTime.UtcNow);
        }

        // 02 - Authentication: Login Denied for Disabled User
        [TestMethod]
        public async Task Login_ForDisabledUser_IsRejected()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await SeedUserAsync(context, "disabled.user@hospital.local", "Password123!", disabled: true);
            var service = new AuthService(new UsersRepository(context), BuildConfiguration());

            await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
                async () => await service.LoginAsync(
                    new LoginRequest { Email = "disabled.user@hospital.local", Password = "Password123!" }));
        }

        // 03 - ER Visit: Create and Complete Flow
        [TestMethod]
        public async Task ERVisit_CreateTriageAndComplete_EndsClosed()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var service = new ERVisitService(
                new ERVisitRepository(context),
                new ERRoomRepository(context),
                new TriageRepository(context),
                new TriageParametersRepository(context),
                new TransferLogRepository(context),
                new PatientRepository(context));

            ERVisit created = await service.CreateAsync(new ERVisit
            {
                Patient = NewPatient("Erin"),
                ChiefComplaint = "Severe abdominal pain",
                ArrivalDateTime = DateTime.Now,
                Status = ERVisit.VisitStatus.REGISTERED,
            });

            created.Status = ERVisit.VisitStatus.TRIAGED;
            await service.UpdateAsync(created);

            await service.CloseVisitAsync(created.VisitId);

            ERVisit? closed = await service.GetByIdAsync(created.VisitId);
            Assert.IsNotNull(closed);
            Assert.AreEqual(ERVisit.VisitStatus.CLOSED, closed!.Status);
        }

        // 08 - Patient: Edit and Validate Profile
        [TestMethod]
        public async Task Patient_EditProfile_ValidPhoneIsPersisted()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var repository = new PatientRepository(context);
            Patient patient = await repository.CreateAsync(NewPatient("Pat"));

            patient.PhoneNumber = "0712345678";
            Assert.IsTrue(patient.Validate(out _));
            await repository.UpdateAsync(patient);

            Patient? reloaded = await repository.GetByIdAsync(patient.PatientId);
            Assert.IsNotNull(reloaded);
            Assert.AreEqual("0712345678", reloaded!.PhoneNumber);
        }

        [TestMethod]
        public async Task Patient_EditProfile_InvalidCnpFailsValidation()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var repository = new PatientRepository(context);
            Patient patient = await repository.CreateAsync(NewPatient("Pat"));

            patient.Cnp = "abc";
            bool isValid = patient.Validate(out List<string> errors);

            Assert.IsFalse(isValid);
            CollectionAssert.Contains(errors, "Patient ID (CNP) must be exactly 13 digits.");
        }

        // 11 - Transplant: Register Donor and Recipient (O+ donor compatible with A+ recipient)
        [TestMethod]
        public void Transplant_RegisterDonorAndRecipient_OPositiveCompatibleWithAPositive()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var service = new BloodCompatibilityService(
                new PatientRepository(context),
                new MedicalHistoryRepository(context));

            Assert.IsTrue(service.IsBloodMatch(BloodType.O, BloodType.A));
            Assert.IsTrue(service.IsRhMatch(Rh.Positive, Rh.Positive));
        }

        [TestMethod]
        public async Task Transplant_RegisterDonorAndRecipient_ReturnsCompatibleDonorForRecipient()
        {
            using var context = IntegrationTestContextFactory.CreateContext();

            var recipient = NewPatient("Recipient");
            var recipientHistory = new MedicalHistory { Patient = recipient, BloodType = BloodType.A, Rh = Rh.Positive };
            var donor = NewPatient("Donor");
            var donorHistory = new MedicalHistory { Patient = donor, BloodType = BloodType.O, Rh = Rh.Positive };
            context.Patients.AddRange(recipient, donor);
            context.MedicalHistories.AddRange(recipientHistory, donorHistory);
            await context.SaveChangesAsync();

            var service = new BloodCompatibilityService(
                new PatientRepository(context),
                new MedicalHistoryRepository(context));

            List<Patient> donors = await service.GetTopCompatibleDonorsAsync(recipient.PatientId);

            Assert.AreEqual(1, donors.Count);
            Assert.AreEqual("Donor", donors[0].FirstName);
        }

        // 12 - Billing: Apply and Persist Discount (25%)
        [TestMethod]
        public async Task Billing_ApplyAndPersistDiscount_UpdatesFinalPrice()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var patient = NewPatient("Bill");
            var history = new MedicalHistory { Patient = patient };
            var record = new MedicalRecord
            {
                MedicalHistory = history,
                SourceType = SourceType.ER,
                ConsultationDate = DateTime.Today,
                BasePrice = 0,
                FinalPrice = 0,
            };
            context.MedicalRecords.Add(record);
            await context.SaveChangesAsync();

            var service = new BillingService(
                new MedicalHistoryRepository(context),
                new MedicalRecordRepository(context),
                new PrescriptionRepository(context),
                new TransplantRepository(context));

            decimal final = await service.PersistDiscountAsync(record.RecordId, 1000m, 25);

            Assert.AreEqual(750m, final);
            Assert.AreEqual(
                750m,
                (await new MedicalRecordRepository(context).GetByIdAsync(record.RecordId))!.FinalPrice);
        }
    }
}
