using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class SchedulingServiceTests
    {
        [TestMethod]
        public void AddItemToBasket_WhenNew_AddsEntry()
        {
            var u = new User();

            u.AddItemToBasket(11, 2);

            Assert.IsTrue(u.Basket.ContainsKey(11));
        }

        [TestMethod]
        public void ChangeItemQuantityInBasket_WhenNotExists_Throws()
        {
            var u = new User();

            Assert.ThrowsException<System.ArgumentException>(() => u.ChangeItemQuantityInBasket(99, 5));
        }
    }
}
