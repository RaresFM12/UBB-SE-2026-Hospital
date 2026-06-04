using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class OrderServiceTests
    {
        [TestMethod]
        public void AddItemToOrder_WhenNewItem_AddsEntry()
        {
            var order = new Order(1, new User(), DateOnly.FromDateTime(DateTime.UtcNow));

            order.AddItemToOrder(5, 2, 12.5f);

            Assert.IsTrue(order.ItemQuantitiesWithFinalPrice.ContainsKey(5));
        }

        [TestMethod]
        public void AddItemToOrder_WhenDuplicate_ThrowsArgumentException()
        {
            var order = new Order(2, new User(), DateOnly.FromDateTime(DateTime.UtcNow));
            order.AddItemToOrder(1, 1, 5f);

            Assert.ThrowsException<ArgumentException>(() => order.AddItemToOrder(1, 2, 6f));
        }

        [TestMethod]
        public void ChangeItemInfoInOrder_WhenExists_UpdatesQuantity()
        {
            var order = new Order(3, new User(), DateOnly.FromDateTime(DateTime.UtcNow));
            order.AddItemToOrder(9, 3, 7.5f);

            order.ChangeItemInfoInOrder(9, 5, 8.0f);

            Assert.AreEqual(5, order.ItemQuantitiesWithFinalPrice[9].Item1);
        }
    }
}
