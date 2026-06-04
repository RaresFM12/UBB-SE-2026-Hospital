using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using System;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class MoreOrderModelTests
    {
        [TestMethod]
        public void T119_Order_AddMultipleItems_TracksBoth()
        {
            var o = new Order(900, new User(), DateOnly.FromDateTime(DateTime.UtcNow));
            o.AddItemToOrder(1, 1, 5f);
            o.AddItemToOrder(2, 2, 3f);
            Assert.IsTrue(o.ItemQuantitiesWithFinalPrice.ContainsKey(1) && o.ItemQuantitiesWithFinalPrice.ContainsKey(2));
        }

        [TestMethod]
        public void T120_Order_ChangeItemInfo_Updates()
        {
            var o = new Order(901, new User(), DateOnly.FromDateTime(DateTime.UtcNow));
            o.AddItemToOrder(10, 1, 2f);
            o.ChangeItemInfoInOrder(10, 3, 1.5f);
            // ItemQuantitiesWithFinalPrice stores Tuple<int, float> (quantity, price)
            Assert.AreEqual(3, o.ItemQuantitiesWithFinalPrice[10].Item1);
        }

        [TestMethod]
        public void T121_Order_RemoveItem_Removes()
        {
            var o = new Order(902, new User(), DateOnly.FromDateTime(DateTime.UtcNow));
            o.AddItemToOrder(11, 1, 2f);
            o.RemoveItemFromOrder(11);
            Assert.IsFalse(o.ItemQuantitiesWithFinalPrice.ContainsKey(11));
        }

        [TestMethod]
        public void T122_Order_Equals_Null_False()
        {
            var o = new Order(903, new User(), DateOnly.FromDateTime(DateTime.UtcNow));
            Assert.IsFalse(o.Equals(null));
        }

        [TestMethod]
        public void T123_Order_PickUpAndExpirationStrings()
        {
            var pick = DateOnly.FromDateTime(new DateTime(2026, 2, 2));
            var o = new Order(904, new User(), pick);
            Assert.IsTrue(o.PickUpDateString.Contains("2026") && o.ExpirationDateString.Contains("2026"));
        }
    }
}
