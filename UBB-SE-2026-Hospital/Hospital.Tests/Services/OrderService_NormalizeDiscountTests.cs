using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using Hospital.Services;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class OrderService_NormalizeDiscountTests
    {
        [TestMethod]
        public void NormalizeDiscount_AboveOne_DividesBy100()
        {
            var result = (float)typeof(OrderService).GetMethod("NormalizeDiscount", BindingFlags.NonPublic | BindingFlags.Static)!
                .Invoke(null, new object[] { 150f });
            Assert.AreEqual(1f, result);
        }

        [TestMethod]
        public void NormalizeDiscount_Negative_ClampsToZero()
        {
            var result = (float)typeof(OrderService).GetMethod("NormalizeDiscount", BindingFlags.NonPublic | BindingFlags.Static)!
                .Invoke(null, new object[] { -0.5f });
            Assert.AreEqual(0f, result);
        }
    }
}
