using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using System;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class MoreUserModelTests
    {
        [TestMethod]
        public void T101_User_AddMultipleStockAlerts_Works()
        {
            var u = new User();
            u.AddStockAlertToUser(1);
            u.AddStockAlertToUser(2);
            u.AddStockAlertToUser(3);
            Assert.AreEqual(3, u.StockAlerts.Count);
        }

        [TestMethod]
        public void T102_User_RemoveStockAlert_NotAffectOthers()
        {
            var u = new User();
            u.AddStockAlertToUser(10);
            u.AddStockAlertToUser(11);
            u.RemoveStockAlertFromUser(10);
            Assert.IsTrue(u.StockAlerts.Contains(11) && !u.StockAlerts.Contains(10));
        }

        [TestMethod]
        public void T103_User_AddFavoriteMultiple_Works()
        {
            var u = new User();
            u.AddItemToFavoriteItems(5);
            u.AddItemToFavoriteItems(6);
            Assert.AreEqual(2, u.FavoriteItems.Count);
        }

        [TestMethod]
        public void T104_User_Basket_AddAndRemove_ItemNotInBasket()
        {
            var u = new User();
            u.AddItemToBasket(200, 2);
            u.RemoveItemFromBasket(200);
            Assert.IsFalse(u.Basket.ContainsKey(200));
        }

        [TestMethod]
        public void T105_User_ChangeBasketQuantity_Updates()
        {
            var u = new User();
            u.AddItemToBasket(201, 1);
            u.ChangeItemQuantityInBasket(201, 4);
            Assert.AreEqual(4, u.Basket[201].Quantity);
        }

        [TestMethod]
        public void T106_User_AddUserDiscount_AndRemove()
        {
            var u = new User();
            u.AddUserDiscount(50, 0.15f);
            Assert.IsTrue(u.UserDiscounts.ContainsKey(50));
            u.RemoveUserDiscount(50);
            Assert.IsFalse(u.UserDiscounts.ContainsKey(50));
        }

        [TestMethod]
        public void T107_User_SetPeriodTracker_StartDateStored()
        {
            var u = new User();
            var date = DateOnly.FromDateTime(new DateTime(2025, 5, 1));
            u.SetPeriodTracker(date, 29, 3, 1);
            Assert.AreEqual(date, u.StartPeriodDate);
        }

        [TestMethod]
        public void T108_User_AddPeriodNote_PersistsText()
        {
            var u = new User();
            u.AddPeriodNoteToUser(7, "symptom", true);
            // PeriodNotes stores Tuple<string,bool> => Item1 is the text
            Assert.AreEqual("symptom", u.PeriodNotes[7].Item1);
        }

        [TestMethod]
        public void T109_User_AddDuplicateDiscount_Throws()
        {
            var u = new User();
            u.AddUserDiscount(60, 0.1f);
            Assert.ThrowsException<ArgumentException>(() => u.AddUserDiscount(60, 0.2f));
        }

        [TestMethod]
        public void T110_User_RemovePeriodNote_NotExist_Throws()
        {
            var u = new User();
            Assert.ThrowsException<ArgumentException>(() => u.RemovePeriodNoteFromUser(9999));
        }
    }
}
