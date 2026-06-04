using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using System.Collections.Generic;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class LabServiceTests
    {
        [TestMethod]
        public void Item_RemoveBatchFromItem_UpdatesQuantity()
        {
            var item = new Item(1, "Lab", "X", "Y", 1f, 10);
            var d = DateOnly.FromDateTime(System.DateTime.UtcNow.AddDays(30));
            item.AddNewBatchToItem(d, 10);

            item.RemoveBatchFromItem(d);

            Assert.AreEqual(0, item.Quantity);
        }

        [TestMethod]
        public void ChangeNumberOfPacksForBatch_WhenNonexistent_Throws()
        {
            var item = new Item(4, "L", "X", "Y", 1f, 10);
            var d = DateOnly.FromDateTime(System.DateTime.UtcNow.AddDays(30));

            Assert.ThrowsException<ArgumentException>(() => item.ChangeNumberOfPacksForBatch(d, 5));
        }
    }
}
