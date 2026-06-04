using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class PaymentServiceTests
    {
        [TestMethod]
        public void User_AddUserDiscount_WhenNotExists_Adds()
        {
            var u = new User();

            u.AddUserDiscount(10, 0.2f);

            Assert.IsTrue(u.UserDiscounts.ContainsKey(10));
        }

        [TestMethod]
        public void ChangeUserDiscount_WhenNotExists_Throws()
        {
            var u = new User();

            Assert.ThrowsException<System.ArgumentException>(() => u.ChangeUserDiscount(1, 0.5f));
        }

        [TestMethod]
        public void RemoveUserDiscount_WhenNotExists_Throws()
        {
            var u = new User();

            Assert.ThrowsException<System.ArgumentException>(() => u.RemoveUserDiscount(1));
        }
    }
}
