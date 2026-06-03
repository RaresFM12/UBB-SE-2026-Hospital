using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using System;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class ReportingServiceTests
    {
        [TestMethod]
        public void Order_IdString_IncludesId()
        {
            var order = new Order(7, new User(), DateOnly.FromDateTime(DateTime.UtcNow));

            Assert.IsTrue(order.IdString.Contains("7"));
        }

        [TestMethod]
        public void Equals_SameId_ReturnsTrue()
        {
            var o1 = new Order(5, new User(), DateOnly.FromDateTime(DateTime.UtcNow));
            var o2 = new Order(5, new User(), DateOnly.FromDateTime(DateTime.UtcNow));

            Assert.IsTrue(o1.Equals(o2));
        }
    }
}
