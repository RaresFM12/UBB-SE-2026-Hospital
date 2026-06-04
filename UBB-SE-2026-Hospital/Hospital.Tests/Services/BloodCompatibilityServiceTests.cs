using System;
using System.Collections.Generic;
using System.Linq;
using Hospital.Services.PatientEr;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Hospital.Tests.Services
{
    [TestClass]
    public sealed class BloodCompatibilityServiceTests
    {
        private readonly Mock<IPatientRepository> patientRepo = new();
        private readonly Mock<IMedicalHistoryRepository> historyRepo = new();

        private BloodCompatibilityService CreateService() => new BloodCompatibilityService(patientRepo.Object, historyRepo.Object);

        [TestMethod]
        public async Task GetTopCompatibleDonorsAsync_WhenRecipientDoesNotExist_ReturnsEmptyList()
        {
            var sut = CreateService();
            patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Patient?)null);

            var result = await sut.GetTopCompatibleDonorsAsync(1);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void IsBloodMatch_O_Donor_ReturnsTrueForAnyReceiver()
        {
            var svc = CreateService();

            Assert.IsTrue(svc.IsBloodMatch(BloodType.O, BloodType.AB));
        }

        [TestMethod]
        public void IsBloodMatch_A_Donor_NotMatchBReceiver_ReturnsFalse()
        {
            var svc = CreateService();

            Assert.IsFalse(svc.IsBloodMatch(BloodType.A, BloodType.B));
        }

        [TestMethod]
        public void IsRhMatch_ReceiverNegative_DonorPositive_ReturnsFalse()
        {
            var svc = CreateService();

            Assert.IsFalse(svc.IsRhMatch(Rh.Positive, Rh.Negative));
        }
    }
}
