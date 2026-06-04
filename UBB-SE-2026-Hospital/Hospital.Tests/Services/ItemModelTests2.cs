using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using System;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class ItemModelTests2
    {
        [TestMethod]
        public void AddNewBatchToItem_MultipleBatches_IncreasesQuantity()
        {
            var item = new Item(10, "X", "Y", "Z", 1f, 10);
            item.AddNewBatchToItem(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), 2);
            item.AddNewBatchToItem(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)), 3);
            Assert.AreEqual(5, item.Quantity);
        }

        [TestMethod]
        public void GetQuantityAtSpecifiedDate_NoBatches_ReturnsZero()
        {
            var item = new Item(11, "X", "Y", "Z", 1f, 10);
            var q = item.GetQuantityAtSpecifiedDate(DateOnly.FromDateTime(DateTime.UtcNow));
            Assert.AreEqual(0, q);
        }

        [TestMethod]
        public void RemoveQuantityFromItem_PartialRemoves_AdjustsQuantity()
        {
            var item = new Item(12, "A", "B", "C", 2f, 10);
            var d1 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
            var d2 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15));
            item.AddNewBatchToItem(d1, 1);
            item.AddNewBatchToItem(d2, 3);
            item.RemoveQuantityFromItem(2, DateOnly.FromDateTime(DateTime.UtcNow));
            Assert.AreEqual(2, item.Quantity);
        }
    }
}
