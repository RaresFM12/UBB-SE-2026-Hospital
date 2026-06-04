using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class NotificationServiceTests
    {
        [TestMethod]
        public void Constructor_SetsIsReadFalse()
        {
            var staff = new Staff();
            var n = new Notification("T", "M", "A", staff);

            Assert.IsFalse(n.IsRead);
        }

        [TestMethod]
        public void ToString_NotThrowing()
        {
            var staff = new Staff();
            var n = new Notification("T", "M", "A", staff);

            var s = n.ToString();

            Assert.IsTrue(s.Length > 0);
        }
    }
}
