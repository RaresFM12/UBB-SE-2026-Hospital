using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using Hospital.Services.PatientEr;
using Hospital.Data.Repositories;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class BloodCompatibilityServiceTests2
    {
        [TestMethod]
        public void CalculateScore_WhenNullHistories_ReturnsZero()
        {
            var repo = new Mock<IPatientRepository>();
            var hist = new Mock<IMedicalHistoryRepository>();
            var svc = new BloodCompatibilityService(repo.Object, hist.Object);

            var donor = new Patient();
            var receiver = new Patient();

            var result = svc.CalculateScore(donor, receiver);
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void CalculateScore_SameBloodAndSex_IncreasesScore()
        {
            var repo = new Mock<IPatientRepository>();
            var hist = new Mock<IMedicalHistoryRepository>();
            var svc = new BloodCompatibilityService(repo.Object, hist.Object);

            var donor = new Patient { MedicalHistory = new MedicalHistory { BloodType = BloodType.A, Rh = Rh.Positive }, DateOfBirth = new System.DateTime(1980,1,1), Sex = Sex.M };
            var receiver = new Patient { MedicalHistory = new MedicalHistory { BloodType = BloodType.A, Rh = Rh.Positive }, DateOfBirth = new System.DateTime(1985,1,1), Sex = Sex.M };

            var result = svc.CalculateScore(donor, receiver);
            Assert.IsTrue(result > 0);
        }
    }
}
