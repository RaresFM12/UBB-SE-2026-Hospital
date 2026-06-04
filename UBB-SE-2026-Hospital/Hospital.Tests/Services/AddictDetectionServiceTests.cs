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
    public sealed class AddictDetectionServiceTests
    {
        [TestMethod]
        public async Task GetAddictCandidatesAsync_WhenNoFlaggedPrescriptions_ReturnsEmptyList()
        {
            var presRepo = new Mock<IPrescriptionRepository>();
            var historyRepo = new Mock<IMedicalHistoryRepository>();
            presRepo.Setup(p => p.GetPotentialDrugAddictsAsync()).ReturnsAsync(new List<Prescription>());

            var sut = new AddictDetectionService(presRepo.Object, historyRepo.Object);

            var result = await sut.GetAddictCandidatesAsync();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task MarkPoliceNotifiedAsync_InvalidId_ThrowsArgumentException()
        {
            var presRepo = new Mock<IPrescriptionRepository>();
            var historyRepo = new Mock<IMedicalHistoryRepository>();
            var sut = new AddictDetectionService(presRepo.Object, historyRepo.Object);

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await sut.MarkPoliceNotifiedAsync(0));
        }
    }
}
