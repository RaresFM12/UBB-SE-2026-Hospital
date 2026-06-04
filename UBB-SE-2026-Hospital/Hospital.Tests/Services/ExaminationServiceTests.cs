using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Services.PatientEr;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Hospital.Tests.Services
{
    [TestClass]
    public sealed class ExaminationServiceTests
    {
        private Mock<IExaminationRepository> repository = null!;
        private Mock<IERVisitRepository> visitRepository = null!;
        private Mock<IERRoomRepository> roomRepository = null!;
        private Mock<ITriageRepository> triageRepository = null!;
        private Mock<ITriageParametersRepository> triageParametersRepository = null!;
        private Mock<IPatientRepository> patientRepository = null!;
        private ExaminationService sut = null!;

        [TestInitialize]
        public void Setup()
        {
            repository = new Mock<IExaminationRepository>();
            visitRepository = new Mock<IERVisitRepository>();
            roomRepository = new Mock<IERRoomRepository>();
            triageRepository = new Mock<ITriageRepository>();
            triageParametersRepository = new Mock<ITriageParametersRepository>();
            patientRepository = new Mock<IPatientRepository>();
            // ExaminationService constructor takes 5 dependencies in this workspace
            sut = new ExaminationService(
                repository.Object,
                visitRepository.Object,
                roomRepository.Object,
                triageRepository.Object,
                triageParametersRepository.Object);
        }

        private static Patient MakePatient(string cnp = "1234567890123") => new()
        {
            PatientId = 7,
            FirstName = "Jane",
            LastName = "Doe",
            Cnp = cnp,
            PhoneNumber = "0700",
            EmergencyContact = "John",
            DateOfBirth = new DateTime(1990, 1, 1),
            Sex = Sex.F
        };

        [TestMethod]
        public async Task GetByVisitIdAsync_WhenMultipleExaminationsExist_ReturnsNewestFirst()
        {
            repository.Setup(x => x.GetByVisitIdAsync(10)).ReturnsAsync(new List<Examination>
            {
                new Examination { ExaminationId = 1, Visit = new ERVisit { VisitId = 10 }, ExaminationDate = new DateTime(2026,1,1) },
                new Examination { ExaminationId = 2, Visit = new ERVisit { VisitId = 10 }, ExaminationDate = new DateTime(2026,1,2) }
            });

            var result = await sut.GetByVisitIdAsync(10);

            Assert.AreEqual(2, result[0].ExaminationId);
        }

        [TestMethod]
        public async Task GetAllAsync_WhenRepositoryReturnsItems_ReturnsAllItems()
        {
            repository.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Examination>
            {
                new Examination { ExaminationId = 1 },
                new Examination { ExaminationId = 2 }
            });

            var result = await sut.GetAllAsync();

            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task GetPatientHistoryAsync_WhenPatientHasNoVisits_ReturnsEmptyList()
        {
            int patientId = 123;
            visitRepository.Setup(x => x.GetByPatientIdAsync(patientId)).ReturnsAsync(new List<ERVisit>());

            var result = await sut.GetPatientHistoryAsync(patientId);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetPatientHistoryAsync_WhenVisitsExist_ReturnsNewestExaminationFirst()
        {
            int patientId = 123;
            var visits = new List<ERVisit>
            {
                new ERVisit { VisitId = 1, Patient = MakePatient() },
                new ERVisit { VisitId = 2, Patient = MakePatient() }
            };
            visitRepository.Setup(x => x.GetByPatientIdAsync(patientId)).ReturnsAsync(visits);

            repository.Setup(x => x.GetByVisitIdAsync(1)).ReturnsAsync(new List<Examination>
            {
                new Examination { ExaminationId = 1, Visit = visits[0], ExaminationDate = new DateTime(2026,1,1) }
            });
            repository.Setup(x => x.GetByVisitIdAsync(2)).ReturnsAsync(new List<Examination>
            {
                new Examination { ExaminationId = 2, Visit = visits[1], ExaminationDate = new DateTime(2026,1,3) }
            });

            var result = await sut.GetPatientHistoryAsync(patientId);

            Assert.AreEqual(2, result[0].ExaminationId);
        }
    }
}
