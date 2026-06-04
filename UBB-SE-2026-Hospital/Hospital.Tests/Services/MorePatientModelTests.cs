using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using System;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class MorePatientModelTests
    {
        [TestMethod]
        public void T131_Patient_GetAge_LeapYear()
        {
            var p = new Patient { DateOfBirth = new DateTime(2004, 2, 29) };
            var age = p.GetAge();
            Assert.IsTrue(age >= 18);
        }

        [TestMethod]
        public void T132_Patient_Cnp_NotEmptyByDefault()
        {
            var p = new Patient();
            // Patient.Cnp defaults to empty string in this model; validate that Validate() reports an error
            var ok = p.Validate(out var errors);
            Assert.IsFalse(ok);
            Assert.IsTrue(errors.Exists(e => e.Contains("Patient ID")));
        }

        [TestMethod]
        public void T133_Patient_FullName_HandlesNullsAndSpaces()
        {
            var p = new Patient { FirstName = " A ", LastName = " B " };
            Assert.AreEqual(" A   B ".Trim(), p.FullName.Trim());
        }
    }
}
