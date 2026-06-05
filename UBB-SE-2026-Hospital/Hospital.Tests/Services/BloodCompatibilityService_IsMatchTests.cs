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
    public class BloodCompatibilityService_IsMatchTests
    {
        [TestMethod]
        public void IsBloodMatch_O_to_AB_ReturnsTrue()
        {
            var svc = new BloodCompatibilityService(null!, null!);
            Assert.IsTrue(svc.IsBloodMatch(BloodType.O, BloodType.AB));
        }

        [TestMethod]
        public void IsRhMatch_PositiveReceiver_True()
        {
            var svc = new BloodCompatibilityService(null!, null!);
            Assert.IsTrue(svc.IsRhMatch(Rh.Positive, Rh.Positive));
        }

        [TestMethod]
        public void CalculateScore_DifferentSexAndAge_GeneratesScore()
        {
            var svc = new BloodCompatibilityService(null!, null!);
            var donor = new Patient { DateOfBirth = new DateTime(1980,1,1), Sex = Sex.M, MedicalHistory = new MedicalHistory { BloodType = BloodType.A, Rh = Rh.Positive } };
            var recipient = new Patient { DateOfBirth = new DateTime(1990,1,1), Sex = Sex.F, MedicalHistory = new MedicalHistory { BloodType = BloodType.A, Rh = Rh.Positive } };
            var score = svc.CalculateScore(donor, recipient);
            Assert.IsTrue(score > 0);
        }
    }
}
