using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using System;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class NotificationModelTests2
    {
        [TestMethod]
        public void NewNotification_HasCreatedAt()
        {
            var s = new Staff();
            var n = new Notification("T","M","A", s);
            Assert.IsTrue(n.CreatedAt <= DateTime.UtcNow);
        }

        [TestMethod]
        public void ToggleIsReadProperty_Works()
        {
            var s = new Staff();
            var n = new Notification("t","m","a", s);
            n.IsRead = true;
            Assert.IsTrue(n.IsRead);
        }
    }
}
