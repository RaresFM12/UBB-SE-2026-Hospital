using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Services.PatientEr;
using Hospital.Data.Repositories;
using Moq;
using System.Threading.Tasks;
using Hospital.Data.Models;
using System.Collections.Generic;
using System;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class ExaminationServiceTests3
    {
        [TestMethod]
        public async Task DeleteAsync_CallsRepository()
        {
            var repo = new Mock<IExaminationRepository>();
            var visitRepo = new Mock<IERVisitRepository>();
            var roomRepo = new Mock<IERRoomRepository>();
            var triage = new Mock<ITriageRepository>();
            var triageParams = new Mock<ITriageParametersRepository>();

            repo.Setup(r => r.DeleteAsync(5)).Returns(Task.CompletedTask).Verifiable();
            var svc = new ExaminationService(repo.Object, visitRepo.Object, roomRepo.Object, triage.Object, triageParams.Object);
            await svc.DeleteAsync(5);
            repo.Verify();
        }
    }
}
