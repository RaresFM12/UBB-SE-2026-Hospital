using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using System;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class MoreStaffModelTests
    {
        [TestMethod]
        public void T134_Staff_FullName_EmptyLast_ReturnsFirst()
        {
            var s = new Staff { FirstName = "Only", LastName = "" };
            Assert.AreEqual("Only", s.FullName);
        }

        [TestMethod]
        public void T135_Staff_LicenseNumber_DefaultEmpty()
        {
            var s = new Staff();
            Assert.AreEqual(string.Empty, s.LicenseNumber);
        }
    }
}
