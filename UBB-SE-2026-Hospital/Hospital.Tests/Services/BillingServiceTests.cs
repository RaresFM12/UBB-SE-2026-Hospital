using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using System;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class BillingServiceTests
    {
        [TestMethod]
        public void OrderExpirationDateString_ComputesCorrectly()
        {
            var user = new User();
            var pickUp = DateOnly.FromDateTime(new DateTime(2026,6,1));
            var order = new Order(3, user, pickUp);

            Assert.IsTrue(order.ExpirationDateString.Contains("2026"));
        }

        [TestMethod]
        public void PickUpDateString_FormatIsYearMonthDay()
        {
            var user = new User();
            var pickUp = DateOnly.FromDateTime(new DateTime(2026,6,15));
            var order = new Order(4, user, pickUp);

            Assert.AreEqual("2026.06.15", order.PickUpDateString);
        }
    }
}
