using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Services;
using Hospital.Data.Models;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class OrderService_ParsePrescriptionQuantityTests
    {
        [TestMethod]
        public void ParsePrescriptionQuantity_NullOrEmpty_ReturnsDefault()
        {
            var parsed = typeof(OrderService).GetMethod("ParsePrescriptionQuantity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object?[] { null });
            Assert.AreEqual(1, parsed);
        }

        [TestMethod]
        public void ParsePrescriptionQuantity_WithDigits_ReturnsParsed()
        {
            var parsed = typeof(OrderService).GetMethod("ParsePrescriptionQuantity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object?[] { "x12y" });
            Assert.AreEqual(12, parsed);
        }

        [TestMethod]
        public void ParsePrescriptionQuantity_NonDigits_ReturnsDefault()
        {
            var parsed = typeof(OrderService).GetMethod("ParsePrescriptionQuantity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object?[] { "abc" });
            Assert.AreEqual(1, parsed);
        }
    }
}
