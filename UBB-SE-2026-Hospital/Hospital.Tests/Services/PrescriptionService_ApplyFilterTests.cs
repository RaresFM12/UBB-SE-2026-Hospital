using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Services;
using Hospital.Data.Repositories;
using Hospital.Data.Models;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class PrescriptionService_ApplyFilterTests
    {
        [TestMethod]
        public async Task ApplyFilter_WithFilter_ReturnsFiltered()
        {
            var mockPresRepo = new Mock<IPrescriptionRepository>();
            mockPresRepo.Setup(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>())).ReturnsAsync(new List<Prescription> { new Prescription() });
            var service = new PrescriptionService(mockPresRepo.Object);
            var res = await service.ApplyFilterAsync(new PrescriptionFilter { PatientId = 1 });
            Assert.AreEqual(1, res.Count);
        }

        [TestMethod]
        public async Task ApplyFilter_NullFilter_ReturnsTopN()
        {
            var mockPresRepo = new Mock<IPrescriptionRepository>();
            mockPresRepo.Setup(r => r.GetTopNAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new List<Prescription>());
            var service = new PrescriptionService(mockPresRepo.Object);
            var res = await service.ApplyFilterAsync(null);
            Assert.IsNotNull(res);
        }
    }
}
