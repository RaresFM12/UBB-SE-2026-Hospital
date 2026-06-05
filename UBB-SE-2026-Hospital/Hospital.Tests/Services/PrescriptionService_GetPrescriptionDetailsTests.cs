using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Services;
using Hospital.Data.Repositories;
using Hospital.Data.Models;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class PrescriptionService_GetPrescriptionDetailsTests
    {
        [TestMethod]
        public async Task GetPrescriptionDetails_Found_ReturnsItem()
        {
            var mockPresRepo = new Mock<IPrescriptionRepository>();
            mockPresRepo.Setup(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>())).ReturnsAsync(new List<Prescription> { new Prescription { PrescriptionId = 99 } });
            var service = new PrescriptionService(mockPresRepo.Object);

            var res = await service.GetPrescriptionDetailsAsync(99);

            Assert.AreEqual(99, res.PrescriptionId);
        }

        [TestMethod]
        public async Task GetPrescriptionDetails_NotFound_ThrowsArgument()
        {
            var mockPresRepo = new Mock<IPrescriptionRepository>();
            mockPresRepo.Setup(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>())).ReturnsAsync(new List<Prescription>());
            var service = new PrescriptionService(mockPresRepo.Object);

            await Assert.ThrowsExceptionAsync(typeof(ArgumentException), async () => await service.GetPrescriptionDetailsAsync(77));
        }
    }
}
