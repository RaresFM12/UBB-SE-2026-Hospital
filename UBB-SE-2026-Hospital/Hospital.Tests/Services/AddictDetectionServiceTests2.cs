using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Services.PatientEr;
using Hospital.Data.Repositories;
using Hospital.Data.Models;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class AddictDetectionServiceTests2
    {
        [TestMethod]
        public async Task BuildPoliceReportAsync_InvalidPatient_Throws()
        {
            var pres = new Mock<IPrescriptionRepository>();
            var mh = new Mock<IMedicalHistoryRepository>();

            pres.Setup(p => p.GetFilteredAsync(It.IsAny<Data.Models.DTOs.PrescriptionFilter>())).ReturnsAsync(new List<Prescription>());

            var svc = new AddictDetectionService(pres.Object, mh.Object);

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await svc.BuildPoliceReportAsync(1));
        }

        [TestMethod]
        public async Task GetChronicConditionsAsync_WhenHistoryNull_ReturnsNoneReported()
        {
            var pres = new Mock<IPrescriptionRepository>();
            var mh = new Mock<IMedicalHistoryRepository>();
            mh.Setup(m => m.GetByPatientIdAsync(It.IsAny<int>())).ReturnsAsync((MedicalHistory?)null);

            var svc = new AddictDetectionService(pres.Object, mh.Object);

            var res = await svc.GetChronicConditionsAsync(1);
            Assert.AreEqual("None reported.", res);
        }
    }
}
