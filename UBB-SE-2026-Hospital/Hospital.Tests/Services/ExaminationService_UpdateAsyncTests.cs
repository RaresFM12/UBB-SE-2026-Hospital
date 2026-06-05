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
    public class ExaminationService_UpdateAsyncTests
    {
        [TestMethod]
        public async Task UpdateAsync_MissingExamination_ThrowsArgument()
        {
            var mockExamRepo = new Mock<IExaminationRepository>();
            var mockVisitRepo = new Mock<IERVisitRepository>();
            var mockRoomRepo = new Mock<IERRoomRepository>();
            var mockTriageRepo = new Mock<ITriageRepository>();
            var mockParamsRepo = new Mock<ITriageParametersRepository>();

            mockExamRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Examination?)null);

            var service = new ExaminationService(mockExamRepo.Object, mockVisitRepo.Object, mockRoomRepo.Object, mockTriageRepo.Object, mockParamsRepo.Object);

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await service.UpdateAsync(new Examination { ExaminationId = 999 }));
        }

        [TestMethod]
        public async Task UpdateAsync_PartialUpdate_UpdatesVisitAndDoctorAndRoom()
        {
            var mockExamRepo = new Mock<IExaminationRepository>();
            var mockVisitRepo = new Mock<IERVisitRepository>();
            var mockRoomRepo = new Mock<IERRoomRepository>();
            var mockTriageRepo = new Mock<ITriageRepository>();
            var mockParamsRepo = new Mock<ITriageParametersRepository>();

            var existing = new Examination { ExaminationId = 1, Visit = new ERVisit { VisitId = 1 }, Doctor = null, Room = null, ExaminationDate = DateTime.UtcNow.AddDays(-1) };
            mockExamRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
            mockExamRepo.Setup(r => r.UpdateAsync(It.IsAny<Examination>())).ReturnsAsync((Examination e) => e);

            var newDoctor = new Staff { StaffId = 7 };
            var newRoom = new ERRoom { RoomId = 99 };
            var updated = new Examination { ExaminationId = 1, Visit = new ERVisit { VisitId = 1 }, Doctor = newDoctor, Room = newRoom, ExaminationDate = DateTime.UtcNow };

            var service = new ExaminationService(mockExamRepo.Object, mockVisitRepo.Object, mockRoomRepo.Object, mockTriageRepo.Object, mockParamsRepo.Object);

            var result = await service.UpdateAsync(updated);

            Assert.AreEqual(newDoctor.StaffId, result.Doctor!.StaffId);
        }

        [TestMethod]
        public async Task UpdateAsync_UpdatesFindingsAndRecommendationAndDate()
        {
            var mockExamRepo = new Mock<IExaminationRepository>();
            var mockVisitRepo = new Mock<IERVisitRepository>();
            var mockRoomRepo = new Mock<IERRoomRepository>();
            var mockTriageRepo = new Mock<ITriageRepository>();
            var mockParamsRepo = new Mock<ITriageParametersRepository>();

            var existing = new Examination { ExaminationId = 2, Findings = "old", Recommendation = "old rec", ExaminationDate = DateTime.UtcNow.AddDays(-2) };
            mockExamRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(existing);
            mockExamRepo.Setup(r => r.UpdateAsync(It.IsAny<Examination>())).ReturnsAsync((Examination e) => e);

            var updated = new Examination { ExaminationId = 2, Findings = "new findings", Recommendation = "new rec", ExaminationDate = DateTime.UtcNow };

            var service = new ExaminationService(mockExamRepo.Object, mockVisitRepo.Object, mockRoomRepo.Object, mockTriageRepo.Object, mockParamsRepo.Object);

            var result = await service.UpdateAsync(updated);

            Assert.AreEqual("new findings", result.Findings);
        }
    }
}
