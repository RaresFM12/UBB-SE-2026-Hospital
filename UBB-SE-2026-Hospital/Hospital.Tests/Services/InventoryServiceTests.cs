using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using System;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class InventoryServiceTests
    {
        [TestMethod]
        public void AddActiveSubstance_WhenDuplicate_Throws()
        {
            var item = new Item(1, "Name", "Prod", "Cat", 1f, 10);
            item.AddActiveSubstanceToItem("sub", 1f);

            Assert.ThrowsException<ArgumentException>(() => item.AddActiveSubstanceToItem("sub", 2f));
        }

        [TestMethod]
        public void ChangeActiveSubstanceConcentration_WhenNotExists_Throws()
        {
            var item = new Item(2, "Name", "Prod", "Cat", 1f, 10);

            Assert.ThrowsException<ArgumentException>(() => item.ChangeActiveSubstanceConcentration("x", 1.2f));
        }

        [TestMethod]
        public void RemoveActiveSubstance_WhenNotExists_Throws()
        {
            var item = new Item(3, "Name", "Prod", "Cat", 1f, 10);

            Assert.ThrowsException<ArgumentException>(() => item.RemoveActiveSubstanceFromItem("x"));
        }
    }
}
