using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using System;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class OrderModelTests2
    {
        [TestMethod]
        public void RemoveItemFromOrder_WhenNotExists_Throws()
        {
            var o = new Order(1, new User(), DateOnly.FromDateTime(DateTime.UtcNow));
            Assert.ThrowsException<ArgumentException>(() => o.RemoveItemFromOrder(99));
        }

        [TestMethod]
        public void ChangeItemInfoInOrder_WhenNotExists_Throws()
        {
            var o = new Order(2, new User(), DateOnly.FromDateTime(DateTime.UtcNow));
            Assert.ThrowsException<ArgumentException>(() => o.ChangeItemInfoInOrder(8, 1, 1f));
        }

        [TestMethod]
        public void AddAndRemoveItem_UpdatesQuantity()
        {
            var o = new Order(3, new User(), DateOnly.FromDateTime(DateTime.UtcNow));
            o.AddItemToOrder(4, 2, 10f);
            o.RemoveItemFromOrder(4);
            Assert.IsFalse(o.ItemQuantitiesWithFinalPrice.ContainsKey(4));
        }
    }
}
