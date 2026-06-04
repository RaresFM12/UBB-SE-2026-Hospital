using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Hospital.Data.Repositories;
using Hospital.Data.Models;
using Hospital.Services.PatientEr;
using Moq;
using System.Collections.Generic;
using System;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class ExaminationServiceTests2
    {
        [TestMethod]
        public async Task GetEligibleVisitsAsync_FiltersByStatus()
        {
            var repo = new Mock<IExaminationRepository>();
            var visitRepo = new Mock<IERVisitRepository>();
            var roomRepo = new Mock<IERRoomRepository>();
            var triageRepo = new Mock<ITriageRepository>();
            var triageParams = new Mock<ITriageParametersRepository>();

            var svc = new ExaminationService(repo.Object, visitRepo.Object, roomRepo.Object, triageRepo.Object, triageParams.Object);

            visitRepo.Setup(v => v.GetAllAsync()).ReturnsAsync(new List<ERVisit> { new ERVisit { VisitId = 1, Status = ERVisit.VisitStatus.WAITING_FOR_DOCTOR, ArrivalDateTime = DateTime.UtcNow } });
            roomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ERRoom>());

            var result = await svc.GetEligibleVisitsAsync();
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task GetSummaryByVisitIdAsync_WhenMissing_ReturnsNull()
        {
            var repo = new Mock<IExaminationRepository>();
            var visitRepo = new Mock<IERVisitRepository>();
            var roomRepo = new Mock<IERRoomRepository>();
            var triageRepo = new Mock<ITriageRepository>();
            var triageParams = new Mock<ITriageParametersRepository>();

            var svc = new ExaminationService(repo.Object, visitRepo.Object, roomRepo.Object, triageRepo.Object, triageParams.Object);

            repo.Setup(r => r.GetByVisitIdAsync(It.IsAny<int>())).ReturnsAsync(new List<Examination>());

            var res = await svc.GetSummaryByVisitIdAsync(1);
            Assert.IsNull(res);
        }
    }
}
