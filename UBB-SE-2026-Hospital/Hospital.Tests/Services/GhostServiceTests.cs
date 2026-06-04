using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;

namespace Hospital.Tests.Services
{
    [TestClass]
    public sealed class GhostServiceTests
    {
        [TestMethod]
        public void AddStockAlertToUser_WhenNew_AddsAlert()
        {
            var user = new User();

            user.AddStockAlertToUser(42);

            Assert.IsTrue(user.StockAlerts.Contains(42));
        }

        [TestMethod]
        public void AddStockAlertToUser_WhenDuplicate_Throws()
        {
            var user = new User();
            user.AddStockAlertToUser(7);

            Assert.ThrowsException<System.ArgumentException>(() => user.AddStockAlertToUser(7));
        }
    }
}
