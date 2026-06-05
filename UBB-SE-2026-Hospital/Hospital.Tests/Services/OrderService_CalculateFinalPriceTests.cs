using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using Hospital.Services;
using Hospital.Data.Models;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class OrderService_CalculateFinalPriceTests
    {
        [TestMethod]
        public void CalculateFinalPrice_NoDiscounts_ComputesBase()
        {
            var item = new Item(1, "A", "P", "C", 10f, 5);
            var result = (float)typeof(OrderService).GetMethod("CalculateFinalPrice", BindingFlags.NonPublic | BindingFlags.Static)!
                .Invoke(null, new object[] { item, 2, 0f, 0f });
            Assert.AreEqual(20f, result);
        }

        [TestMethod]
        public void CalculateFinalPrice_WithDiscounts_ReducesPrice()
        {
            var item = new Item(1, "A", "P", "C", 10f, 5) { DiscountPercentage = 10f };
            var result = (float)typeof(OrderService).GetMethod("CalculateFinalPrice", BindingFlags.NonPublic | BindingFlags.Static)!
                .Invoke(null, new object[] { item, 1, 0.1f, 0f });
            Assert.IsTrue(result < 10f);
        }
    }
}
