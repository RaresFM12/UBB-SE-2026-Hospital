using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using System;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class NotificationServiceTests2
    {
        [TestMethod]
        public void ActionButtonText_DefaultEmpty_WhenConstructed()
        {
            var s = new Staff();
            var n = new Notification("T","M","", s);
            Assert.AreEqual(string.Empty, n.ActionButtonText);
        }
    }
}
