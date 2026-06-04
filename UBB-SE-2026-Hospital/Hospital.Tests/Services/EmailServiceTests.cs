using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class EmailServiceTests
    {
        [TestMethod]
        public void User_AddFavorite_Then_Remove_Works()
        {
            var u = new User();
            u.AddItemToFavoriteItems(3);

            u.RemoveItemFromFavoriteItems(3);

            Assert.IsFalse(u.FavoriteItems.Contains(3));
        }

        [TestMethod]
        public void RemoveItemFromFavoriteItems_WhenNotExists_Throws()
        {
            var u = new User();

            Assert.ThrowsException<System.ArgumentException>(() => u.RemoveItemFromFavoriteItems(5));
        }
    }
}
