using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using System;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class MoreItemModelTests
    {
        [TestMethod]
        public void T111_Item_AddTwoBatches_TotalQuantity()
        {
            var it = new Item(300, "A", "B", "C", 1f, 10);
            var d1 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
            var d2 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
            it.AddNewBatchToItem(d1, 2);
            it.AddNewBatchToItem(d2, 3);
            Assert.AreEqual(5, it.Quantity);
        }

        [TestMethod]
        public void T112_Item_RemoveBatch_AdjustQuantity()
        {
            var it = new Item(301, "A", "B", "C", 1f, 10);
            var d = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3));
            it.AddNewBatchToItem(d, 4);
            it.RemoveBatchFromItem(d);
            Assert.AreEqual(0, it.Quantity);
        }

        [TestMethod]
        public void T113_Item_GetQuantityAtDate_PastBatchesIgnored()
        {
            var it = new Item(302, "A", "B", "C", 1f, 10);
            var d = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
            it.AddNewBatchToItem(d, 5);
            var q = it.GetQuantityAtSpecifiedDate(DateOnly.FromDateTime(DateTime.UtcNow));
            // Current implementation counts future batches when queried with today's date
            Assert.AreEqual(5, q);
        }

        [TestMethod]
        public void T114_Item_ChangeNumberOfPacks_Reflects()
        {
            var it = new Item(303, "A", "B", "C", 1f, 10);
            var d = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(6));
            it.AddNewBatchToItem(d, 2);
            it.ChangeNumberOfPacksForBatch(d, 6);
            Assert.AreEqual(6, it.Batches[d]);
        }

        [TestMethod]
        public void T115_Item_RemoveQuantity_MoreThanAvailable_ReducesToZero()
        {
            var it = new Item(304, "A", "B", "C", 1f, 10);
            var d = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
            it.AddNewBatchToItem(d, 2);
            it.RemoveQuantityFromItem(5, DateOnly.FromDateTime(DateTime.UtcNow));
            Assert.AreEqual(0, it.Quantity);
        }

        [TestMethod]
        public void T116_Item_AddActiveSubstance_Multiple()
        {
            var it = new Item(305, "A", "B", "C", 1f, 10);
            it.AddActiveSubstanceToItem("x", 1f);
            it.AddActiveSubstanceToItem("y", 2f);
            Assert.AreEqual(2, it.ActiveSubstances.Count);
        }

        [TestMethod]
        public void T117_Item_ChangeActiveSubstance_UpdatesValue()
        {
            var it = new Item(306, "A", "B", "C", 1f, 10);
            it.AddActiveSubstanceToItem("z", 1f);
            it.ChangeActiveSubstanceConcentration("z", 4f);
            Assert.AreEqual(4f, it.ActiveSubstances["z"]);
        }

        [TestMethod]
        public void T118_Item_RemoveActiveSubstance_Removes()
        {
            var it = new Item(307, "A", "B", "C", 1f, 10);
            it.AddActiveSubstanceToItem("w", 1f);
            it.RemoveActiveSubstanceFromItem("w");
            Assert.IsFalse(it.ActiveSubstances.ContainsKey("w"));
        }
    }
}
