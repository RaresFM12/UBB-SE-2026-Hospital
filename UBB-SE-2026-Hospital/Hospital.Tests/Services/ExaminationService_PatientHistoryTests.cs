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
    public class ExaminationService_PatientHistoryTests
    {
        [TestMethod]
        public async Task GetPatientHistory_NoVisits_ReturnsEmpty()
        {
            var mockExamRepo = new Mock<IExaminationRepository>();
            var mockVisitRepo = new Mock<IERVisitRepository>();
            var mockRoomRepo = new Mock<IERRoomRepository>();
            var mockTriageRepo = new Mock<ITriageRepository>();
            var mockParamsRepo = new Mock<ITriageParametersRepository>();

            mockVisitRepo.Setup(r => r.GetByPatientIdAsync(It.IsAny<int>())).ReturnsAsync(new List<ERVisit>());

            var service = new ExaminationService(mockExamRepo.Object, mockVisitRepo.Object, mockRoomRepo.Object, mockTriageRepo.Object, mockParamsRepo.Object);

            var result = await service.GetPatientHistoryAsync(1);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetPatientHistory_MultipleVisits_AggregatesAndOrders()
        {
            var mockExamRepo = new Mock<IExaminationRepository>();
            var mockVisitRepo = new Mock<IERVisitRepository>();
            var mockRoomRepo = new Mock<IERRoomRepository>();
            var mockTriageRepo = new Mock<ITriageRepository>();
            var mockParamsRepo = new Mock<ITriageParametersRepository>();

            var visit1 = new ERVisit { VisitId = 10 };
            var visit2 = new ERVisit { VisitId = 11 };
            mockVisitRepo.Setup(r => r.GetByPatientIdAsync(It.IsAny<int>())).ReturnsAsync(new List<ERVisit> { visit1, visit2 });

            var examA = new Examination { ExaminationId = 1, ExaminationDate = DateTime.UtcNow.AddDays(-1) };
            var examB = new Examination { ExaminationId = 2, ExaminationDate = DateTime.UtcNow };
            mockExamRepo.Setup(r => r.GetByVisitIdAsync(10)).ReturnsAsync(new List<Examination> { examA });
            mockExamRepo.Setup(r => r.GetByVisitIdAsync(11)).ReturnsAsync(new List<Examination> { examB });

            var service = new ExaminationService(mockExamRepo.Object, mockVisitRepo.Object, mockRoomRepo.Object, mockTriageRepo.Object, mockParamsRepo.Object);

            var result = await service.GetPatientHistoryAsync(5);

            Assert.AreEqual(2, result.Count);
        }
    }
}
