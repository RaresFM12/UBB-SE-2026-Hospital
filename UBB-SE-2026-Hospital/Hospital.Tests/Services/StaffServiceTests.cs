using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class StaffServiceTests
    {
        [TestMethod]
        public void FullName_ReturnsConcatenatedName()
        {
            var s = new Staff { FirstName = "A", LastName = "B" };

            Assert.AreEqual("A B", s.FullName);
        }

        [TestMethod]
        public void FullName_TrimsExtraSpaces()
        {
            var s = new Staff { FirstName = "A", LastName = "" };

            Assert.AreEqual("A", s.FullName);
        }
    }
}
