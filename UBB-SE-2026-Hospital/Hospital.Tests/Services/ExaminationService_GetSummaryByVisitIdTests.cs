using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Services;
using Hospital.Data.Repositories;
using Hospital.Data.Models;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class ExaminationService_GetSummaryByVisitIdTests
    {
        [TestMethod]
        public async Task GetSummaryByVisitId_NoExamination_ReturnsNull()
        {
            var mockExamRepo = new Mock<IExaminationRepository>();
            var mockVisitRepo = new Mock<IERVisitRepository>();
            var mockRoomRepo = new Mock<IERRoomRepository>();
            var mockTriageRepo = new Mock<ITriageRepository>();
            var mockParamsRepo = new Mock<ITriageParametersRepository>();

            mockExamRepo.Setup(r => r.GetByVisitIdAsync(It.IsAny<int>())).ReturnsAsync(new List<Examination>());
            mockVisitRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ERVisit?)null);

            var service = new ExaminationService(mockExamRepo.Object, mockVisitRepo.Object, mockRoomRepo.Object, mockTriageRepo.Object, mockParamsRepo.Object);

            var res = await service.GetSummaryByVisitIdAsync(1);

            Assert.IsNull(res);
        }

        [TestMethod]
        public async Task GetSummaryByVisitId_AllDataPresent_ReturnsSummary()
        {
            var mockExamRepo = new Mock<IExaminationRepository>();
            var mockVisitRepo = new Mock<IERVisitRepository>();
            var mockRoomRepo = new Mock<IERRoomRepository>();
            var mockTriageRepo = new Mock<ITriageRepository>();
            var mockParamsRepo = new Mock<ITriageParametersRepository>();

            var exam = new Examination { ExaminationId = 5, ExaminationDate = DateTime.UtcNow, Doctor = new Staff { StaffId = 2, FirstName = "Doc", LastName = "Tor" }, Findings = "f" };
            mockExamRepo.Setup(r => r.GetByVisitIdAsync(It.IsAny<int>())).ReturnsAsync(new List<Examination> { exam });

            var patient = new Patient { FirstName = "P", LastName = "L" };
            var visit = new ERVisit { VisitId = 1, Patient = patient, ArrivalDateTime = DateTime.UtcNow, ChiefComplaint = "c" };
            mockVisitRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(visit);

            var triage = new Triage { TriageId = 9, TriageLevel = 1, Specialization = "S" };
            mockTriageRepo.Setup(r => r.GetByVisitIdAsync(It.IsAny<int>())).ReturnsAsync(triage);

            var parameters = new TriageParameters { Consciousness = 1, Breathing = 1, Bleeding = 1, InjuryType = 1, PainLevel = 3 };
            mockParamsRepo.Setup(r => r.GetByTriageIdAsync(It.IsAny<int>())).ReturnsAsync(parameters);

            var service = new ExaminationService(mockExamRepo.Object, mockVisitRepo.Object, mockRoomRepo.Object, mockTriageRepo.Object, mockParamsRepo.Object);

            var res = await service.GetSummaryByVisitIdAsync(1);

            Assert.IsNotNull(res);
        }
    }
}
