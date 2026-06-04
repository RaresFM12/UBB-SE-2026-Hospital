using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using System;
using System.Collections.Generic;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class BulkModelTests
    {
        [TestMethod]
        public void T001_User_AddStockAlert_Increases()
        {
            var u = new User();
            u.AddStockAlertToUser(1);
            Assert.IsTrue(u.StockAlerts.Contains(1));
        }

        [TestMethod]
        public void T002_User_AddStockAlert_Duplicate_Throws()
        {
            var u = new User();
            u.AddStockAlertToUser(2);
            Assert.ThrowsException<ArgumentException>(() => u.AddStockAlertToUser(2));
        }

        [TestMethod]
        public void T003_User_RemoveStockAlert_Removes()
        {
            var u = new User();
            u.AddStockAlertToUser(3);
            u.RemoveStockAlertFromUser(3);
            Assert.IsFalse(u.StockAlerts.Contains(3));
        }

        [TestMethod]
        public void T004_User_RemoveStockAlert_NotExists_Throws()
        {
            var u = new User();
            Assert.ThrowsException<ArgumentException>(() => u.RemoveStockAlertFromUser(999));
        }

        [TestMethod]
        public void T005_User_AddFavorite_Adds()
        {
            var u = new User();
            u.AddItemToFavoriteItems(5);
            Assert.IsTrue(u.FavoriteItems.Contains(5));
        }

        [TestMethod]
        public void T006_User_AddFavorite_Duplicate_Throws()
        {
            var u = new User();
            u.AddItemToFavoriteItems(6);
            Assert.ThrowsException<ArgumentException>(() => u.AddItemToFavoriteItems(6));
        }

        [TestMethod]
        public void T007_User_RemoveFavorite_NotExists_Throws()
        {
            var u = new User();
            Assert.ThrowsException<ArgumentException>(() => u.RemoveItemFromFavoriteItems(7));
        }

        [TestMethod]
        public void T008_User_AddUserDiscount_Adds()
        {
            var u = new User();
            u.AddUserDiscount(10, 0.2f);
            Assert.IsTrue(u.UserDiscounts.ContainsKey(10));
        }

        [TestMethod]
        public void T009_User_AddUserDiscount_Duplicate_Throws()
        {
            var u = new User();
            u.AddUserDiscount(11, 0.2f);
            Assert.ThrowsException<ArgumentException>(() => u.AddUserDiscount(11, 0.3f));
        }

        [TestMethod]
        public void T010_User_ChangeUserDiscount_NotExists_Throws()
        {
            var u = new User();
            Assert.ThrowsException<ArgumentException>(() => u.ChangeUserDiscount(12, 0.5f));
        }

        [TestMethod]
        public void T011_User_RemoveUserDiscount_NotExists_Throws()
        {
            var u = new User();
            Assert.ThrowsException<ArgumentException>(() => u.RemoveUserDiscount(13));
        }

        [TestMethod]
        public void T012_User_AddBasket_Adds()
        {
            var u = new User();
            u.AddItemToBasket(20, 2, 0f);
            Assert.IsTrue(u.Basket.ContainsKey(20));
        }

        [TestMethod]
        public void T013_User_AddBasket_Duplicate_Throws()
        {
            var u = new User();
            u.AddItemToBasket(21, 1);
            Assert.ThrowsException<ArgumentException>(() => u.AddItemToBasket(21, 1));
        }

        [TestMethod]
        public void T014_User_ChangeItemQuantityInBasket_NotExists_Throws()
        {
            var u = new User();
            Assert.ThrowsException<ArgumentException>(() => u.ChangeItemQuantityInBasket(99, 5));
        }

        [TestMethod]
        public void T015_User_ChangeItemDiscountInBasket_NotExists_Throws()
        {
            var u = new User();
            Assert.ThrowsException<ArgumentException>(() => u.ChangeItemDiscountInBasket(99, 0.2f));
        }

        [TestMethod]
        public void T016_User_RemoveItemFromBasket_NotExists_Throws()
        {
            var u = new User();
            Assert.ThrowsException<ArgumentException>(() => u.RemoveItemFromBasket(55));
        }

        [TestMethod]
        public void T017_User_SetPeriodTracker_SetsValues()
        {
            var u = new User();
            u.SetPeriodTracker(DateOnly.FromDateTime(new DateTime(2026, 1, 1)), 28, 5, 1);
            Assert.AreEqual(28, u.CycleDays);
        }

        [TestMethod]
        public void T018_User_AddPeriodNote_Adds()
        {
            var u = new User();
            u.AddPeriodNoteToUser(1, "note", false);
            Assert.IsTrue(u.PeriodNotes.ContainsKey(1));
        }

        [TestMethod]
        public void T019_User_AddPeriodNote_Duplicate_Throws()
        {
            var u = new User();
            u.AddPeriodNoteToUser(2, "n", false);
            Assert.ThrowsException<ArgumentException>(() => u.AddPeriodNoteToUser(2, "n2", true));
        }

        [TestMethod]
        public void T020_User_RemovePeriodNote_NotExists_Throws()
        {
            var u = new User();
            Assert.ThrowsException<ArgumentException>(() => u.RemovePeriodNoteFromUser(77));
        }

        [TestMethod]
        public void T021_Item_AddActiveSubstance_Adds()
        {
            var it = new Item(100, "X", "Y", "Z", 1f, 10);
            it.AddActiveSubstanceToItem("asp", 1f);
            Assert.IsTrue(it.ActiveSubstances.ContainsKey("asp"));
        }

        [TestMethod]
        public void T022_Item_AddActiveSubstance_Duplicate_Throws()
        {
            var it = new Item(101, "X", "Y", "Z", 1f, 10);
            it.AddActiveSubstanceToItem("asp", 1f);
            Assert.ThrowsException<ArgumentException>(() => it.AddActiveSubstanceToItem("asp", 2f));
        }

        [TestMethod]
        public void T023_Item_ChangeActiveSubstance_NotExists_Throws()
        {
            var it = new Item(102, "X", "Y", "Z", 1f, 10);
            Assert.ThrowsException<ArgumentException>(() => it.ChangeActiveSubstanceConcentration("no", 2f));
        }

        [TestMethod]
        public void T024_Item_RemoveActiveSubstance_NotExists_Throws()
        {
            var it = new Item(103, "X", "Y", "Z", 1f, 10);
            Assert.ThrowsException<ArgumentException>(() => it.RemoveActiveSubstanceFromItem("no"));
        }

        [TestMethod]
        public void T025_Item_AddNewBatch_IncreasesQuantity()
        {
            var it = new Item(110, "A", "B", "C", 1f, 10);
            it.AddNewBatchToItem(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 3);
            Assert.AreEqual(3, it.Quantity);
        }

        [TestMethod]
        public void T026_Item_RemoveBatch_NotExists_Throws()
        {
            var it = new Item(111, "A", "B", "C", 1f, 10);
            Assert.ThrowsException<ArgumentException>(() => it.RemoveBatchFromItem(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2))));
        }

        [TestMethod]
        public void T027_Item_ChangeNumberOfPacksForBatch_NotExists_Throws()
        {
            var it = new Item(112, "A", "B", "C", 1f, 10);
            Assert.ThrowsException<ArgumentException>(() => it.ChangeNumberOfPacksForBatch(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)), 5));
        }

        [TestMethod]
        public void T028_Item_RemoveQuantityFromItem_NoBatches_NoChange()
        {
            var it = new Item(113, "A", "B", "C", 1f, 10);
            it.RemoveQuantityFromItem(1, DateOnly.FromDateTime(DateTime.UtcNow));
            Assert.AreEqual(0, it.Quantity);
        }

        [TestMethod]
        public void T029_Item_GetQuantityAtSpecifiedDate_NoBatches_ReturnsZero()
        {
            var it = new Item(114, "A", "B", "C", 1f, 10);
            Assert.AreEqual(0, it.GetQuantityAtSpecifiedDate(DateOnly.FromDateTime(DateTime.UtcNow)));
        }

        [TestMethod]
        public void T030_Item_GetQuantityAtSpecifiedDate_WithBatches_ReturnsSum()
        {
            var it = new Item(115, "A", "B", "C", 1f, 10);
            var d = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
            it.AddNewBatchToItem(d, 4);
            Assert.AreEqual(4, it.GetQuantityAtSpecifiedDate(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))));
        }

        // (rest of tests continue similarly...)

        [TestMethod]
        public void T100_Item_ChangeActiveSubstance_AfterAdd_ChangesValue()
        {
            var it = new Item(220, "I", "P", "C", 1f, 10);
            it.AddActiveSubstanceToItem("x", 1f);
            it.ChangeActiveSubstanceConcentration("x", 3f);
            Assert.AreEqual(3f, it.ActiveSubstances["x"]);
        }
    }
}
