using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using System;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class UserModelTests2
    {
        [TestMethod]
        public void AddStockAlertToUser_AddsDifferentIds()
        {
            var u = new User();
            u.AddStockAlertToUser(1);
            u.AddStockAlertToUser(2);
            Assert.AreEqual(2, u.StockAlerts.Count);
        }

        [TestMethod]
        public void RemoveStockAlertFromUser_Removes()
        {
            var u = new User();
            u.AddStockAlertToUser(4);
            u.RemoveStockAlertFromUser(4);
            Assert.IsFalse(u.StockAlerts.Contains(4));
        }

        [TestMethod]
        public void AddItemToFavoriteItems_ThenDuplicateThrows()
        {
            var u = new User();
            u.AddItemToFavoriteItems(3);
            Assert.ThrowsException<ArgumentException>(() => u.AddItemToFavoriteItems(3));
        }

        [TestMethod]
        public void AddItemToBasket_AddsEntry()
        {
            var u = new User();
            u.AddItemToBasket(10, 2, 0.1f);
            Assert.IsTrue(u.Basket.ContainsKey(10));
        }

        [TestMethod]
        public void ChangeItemDiscountInBasket_WhenNotExists_Throws()
        {
            var u = new User();
            Assert.ThrowsException<ArgumentException>(() => u.ChangeItemDiscountInBasket(99, 0.2f));
        }
    }
}
