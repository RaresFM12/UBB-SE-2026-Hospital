using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using System;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class PharmacyServiceTests
    {
        [TestMethod]
        public void AddNewBatchToItem_IncreasesQuantity()
        {
            var item = new Item(1, "A", "P", "C", 1f, 10);
            item.AddNewBatchToItem(DateOnly.FromDateTime(new DateTime(2026, 7, 1)), 5);
            item.AddNewBatchToItem(DateOnly.FromDateTime(new DateTime(2026, 8, 1)), 3);

            Assert.AreEqual(8, item.Quantity);
        }

        [TestMethod]
        public void GetQuantityAtSpecifiedDate_ReturnsBatchesAfterDate()
        {
            var item = new Item(2, "B", "P", "C", 2f, 10);
            var d1 = DateOnly.FromDateTime(new DateTime(2026, 7, 1));
            var d2 = DateOnly.FromDateTime(new DateTime(2026, 8, 1));
            item.AddNewBatchToItem(d1, 5);
            item.AddNewBatchToItem(d2, 4);

            var q = item.GetQuantityAtSpecifiedDate(DateOnly.FromDateTime(new DateTime(2026, 7, 15)));

            Assert.AreEqual(4, q);
        }

        [TestMethod]
        public void RemoveQuantityFromItem_RemovesFromEarliestNonExpiredBatches()
        {
            var item = new Item(3, "C", "P", "C", 2f, 10);
            var d1 = DateOnly.FromDateTime(new DateTime(2026, 7, 1));
            var d2 = DateOnly.FromDateTime(new DateTime(2026, 8, 1));
            item.AddNewBatchToItem(d1, 2);
            item.AddNewBatchToItem(d2, 5);

            item.RemoveQuantityFromItem(3, DateOnly.FromDateTime(new DateTime(2026, 6, 1)));

            Assert.AreEqual(4, item.Quantity);
        }
    }
}
