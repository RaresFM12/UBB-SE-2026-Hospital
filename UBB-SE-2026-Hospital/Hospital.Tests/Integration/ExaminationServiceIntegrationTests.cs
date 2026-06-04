using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;
using Hospital.Data.Repositories;
using Hospital.Services.PatientEr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hospital.Tests.Integration
{
    // Integration tests for ExaminationService (service -> repositories -> EF Core).
    // Two tests per public service function.
    [TestClass]
    public sealed class ExaminationServiceIntegrationTests
    {
        private static ExaminationService CreateService(Hospital.Data.HospitalDbContext context)
            => new ExaminationService(
                new ExaminationRepository(context),
                new ERVisitRepository(context),
                new ERRoomRepository(context),
                new TriageRepository(context),
                new TriageParametersRepository(context));

        private static Patient NewPatient(string first = "Jane")
            => new Patient
            {
                FirstName = first, LastName = "Doe", Cnp = "1234567890123",
                DateOfBirth = new DateTime(1990, 1, 1), Sex = Sex.F,
                PhoneNumber = "0700000000", EmergencyContact = "X",
            };

        private static ERVisit NewVisit(Patient patient, string status = ERVisit.VisitStatus.IN_ROOM)
            => new ERVisit { Patient = patient, ChiefComplaint = "Pain", ArrivalDateTime = DateTime.Now, Status = status };

        private static Staff NewDoctor()
            => new Doctor { FirstName = "Dr", LastName = "House", Role = "Doctor" };

        private static ERRoom NewRoom()
            => new ERRoom { RoomTypeName = ERRoom.RoomType.GeneralRoom, AvailabilityStatus = ERRoom.RoomStatus.Available };

        private static Examination NewExam(ERVisit visit, Staff doctor, ERRoom room, DateTime date)
            => new Examination { Visit = visit, Doctor = doctor, Room = room, ExaminationDate = date, Findings = "F", Recommendation = "R" };

        private static async Task<(ERVisit visit, Staff doctor, ERRoom room)> SeedContextAsync(
            Hospital.Data.HospitalDbContext context, string visitStatus = ERVisit.VisitStatus.IN_ROOM)
        {
            var patient = NewPatient();
            var visit = NewVisit(patient, visitStatus);
            var doctor = NewDoctor();
            var room = NewRoom();
            context.ERVisits.Add(visit);
            context.Staff.Add(doctor);
            context.ERRooms.Add(room);
            await context.SaveChangesAsync();
            return (visit, doctor, room);
        }

        // ---- GetAllAsync ----
        [TestMethod]
        public async Task GetAllAsync_WhenEmpty_ReturnsEmpty()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Assert.AreEqual(0, (await CreateService(context).GetAllAsync()).Count);
        }

        [TestMethod]
        public async Task GetAllAsync_WhenExaminationsExist_ReturnsAll()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var (visit, doctor, room) = await SeedContextAsync(context);
            context.Examinations.Add(NewExam(visit, doctor, room, DateTime.Now));
            await context.SaveChangesAsync();
            Assert.AreEqual(1, (await CreateService(context).GetAllAsync()).Count);
        }

        // ---- GetByIdAsync ----
        [TestMethod]
        public async Task GetByIdAsync_WhenExists_ReturnsExamination()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var (visit, doctor, room) = await SeedContextAsync(context);
            var exam = NewExam(visit, doctor, room, DateTime.Now);
            context.Examinations.Add(exam);
            await context.SaveChangesAsync();
            Assert.IsNotNull(await CreateService(context).GetByIdAsync(exam.ExaminationId));
        }

        [TestMethod]
        public async Task GetByIdAsync_WhenMissing_ReturnsNull()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Assert.IsNull(await CreateService(context).GetByIdAsync(999));
        }

        // ---- GetByVisitIdAsync ----
        [TestMethod]
        public async Task GetByVisitIdAsync_ReturnsExamsForVisitOrderedByDateDesc()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var (visit, doctor, room) = await SeedContextAsync(context);
            context.Examinations.Add(NewExam(visit, doctor, room, new DateTime(2026, 1, 1)));
            context.Examinations.Add(NewExam(visit, doctor, room, new DateTime(2026, 6, 1)));
            await context.SaveChangesAsync();

            List<Examination> result = await CreateService(context).GetByVisitIdAsync(visit.VisitId);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(new DateTime(2026, 6, 1), result[0].ExaminationDate);
        }

        [TestMethod]
        public async Task GetByVisitIdAsync_WhenNone_ReturnsEmpty()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Assert.AreEqual(0, (await CreateService(context).GetByVisitIdAsync(999)).Count);
        }

        // ---- CreateAsync ----
        [TestMethod]
        public async Task CreateAsync_AssignsIdentity()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var (visit, doctor, room) = await SeedContextAsync(context);
            Examination created = await CreateService(context).CreateAsync(NewExam(visit, doctor, room, DateTime.Now));
            Assert.IsTrue(created.ExaminationId > 0);
        }

        [TestMethod]
        public async Task CreateAsync_PersistsRetrievable()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var (visit, doctor, room) = await SeedContextAsync(context);
            var service = CreateService(context);
            Examination created = await service.CreateAsync(NewExam(visit, doctor, room, DateTime.Now));
            Assert.IsNotNull(await service.GetByIdAsync(created.ExaminationId));
        }

        // ---- UpdateAsync ----
        [TestMethod]
        public async Task UpdateAsync_WhenExists_UpdatesFindings()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var (visit, doctor, room) = await SeedContextAsync(context);
            var exam = NewExam(visit, doctor, room, DateTime.Now);
            context.Examinations.Add(exam);
            await context.SaveChangesAsync();
            exam.Findings = "Updated";
            Examination updated = await CreateService(context).UpdateAsync(exam);
            Assert.AreEqual("Updated", updated.Findings);
        }

        [TestMethod]
        public async Task UpdateAsync_WhenMissing_ThrowsArgumentException()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await CreateService(context).UpdateAsync(new Examination { ExaminationId = 999 }));
        }

        // ---- DeleteAsync ----
        [TestMethod]
        public async Task DeleteAsync_WhenExists_Removes()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var (visit, doctor, room) = await SeedContextAsync(context);
            var exam = NewExam(visit, doctor, room, DateTime.Now);
            context.Examinations.Add(exam);
            await context.SaveChangesAsync();
            var service = CreateService(context);
            await service.DeleteAsync(exam.ExaminationId);
            Assert.IsNull(await service.GetByIdAsync(exam.ExaminationId));
        }

        [TestMethod]
        public async Task DeleteAsync_WhenMissing_DoesNotThrow()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var service = CreateService(context);
            await service.DeleteAsync(999);
            Assert.AreEqual(0, (await service.GetAllAsync()).Count);
        }

        // ---- GetEligibleVisitsAsync ----
        [TestMethod]
        public async Task GetEligibleVisitsAsync_IncludesWaitingForDoctorVisit()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await SeedContextAsync(context, ERVisit.VisitStatus.WAITING_FOR_DOCTOR);
            List<ERVisit> result = await CreateService(context).GetEligibleVisitsAsync();
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task GetEligibleVisitsAsync_ExcludesRegisteredVisit()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            await SeedContextAsync(context, ERVisit.VisitStatus.REGISTERED);
            List<ERVisit> result = await CreateService(context).GetEligibleVisitsAsync();
            Assert.AreEqual(0, result.Count);
        }

        // ---- GetPatientHistoryAsync ----
        [TestMethod]
        public async Task GetPatientHistoryAsync_ReturnsExamsForPatient()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var (visit, doctor, room) = await SeedContextAsync(context);
            context.Examinations.Add(NewExam(visit, doctor, room, DateTime.Now));
            await context.SaveChangesAsync();

            List<Examination> result = await CreateService(context).GetPatientHistoryAsync(visit.Patient.PatientId);

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task GetPatientHistoryAsync_WhenNoVisits_ReturnsEmpty()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Assert.AreEqual(0, (await CreateService(context).GetPatientHistoryAsync(999)).Count);
        }

        // ---- GetSummaryByVisitIdAsync ----
        [TestMethod]
        public async Task GetSummaryByVisitIdAsync_WhenNoExamination_ReturnsNull()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            Assert.IsNull(await CreateService(context).GetSummaryByVisitIdAsync(999));
        }

        [TestMethod]
        public async Task GetSummaryByVisitIdAsync_WhenNoTriage_ReturnsNull()
        {
            using var context = IntegrationTestContextFactory.CreateContext();
            var (visit, doctor, room) = await SeedContextAsync(context);
            context.Examinations.Add(NewExam(visit, doctor, room, DateTime.Now));
            await context.SaveChangesAsync();

            Assert.IsNull(await CreateService(context).GetSummaryByVisitIdAsync(visit.VisitId));
        }
    }
}
