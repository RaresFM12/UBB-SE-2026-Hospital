using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using System;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class MoreNotificationModelTests
    {
        [TestMethod]
        public void T128_Notification_ActionButton_DefaultsToEmpty()
        {
            var s = new Staff();
            var n = new Notification("title", "msg", "", s);
            Assert.AreEqual(string.Empty, n.ActionButtonText);
        }

        [TestMethod]
        public void T129_Notification_MarkRead_Works()
        {
            var s = new Staff();
            var n = new Notification("t", "m", "a", s);
            n.IsRead = true;
            Assert.IsTrue(n.IsRead);
        }

        [TestMethod]
        public void T130_Notification_ToString_NotEmpty()
        {
            var s = new Staff();
            var n = new Notification("t", "m", "a", s);
            Assert.IsTrue(n.ToString().Length > 0);
        }
    }
}
