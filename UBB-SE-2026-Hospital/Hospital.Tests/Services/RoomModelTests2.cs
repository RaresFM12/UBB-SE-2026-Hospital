using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using System;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class RoomModelTests2
    {
        [TestMethod]
        public void NormalizeStatus_ReturnsKnownStatus()
        {
            var s = ERRoom.NormalizeStatus("available");
            Assert.AreEqual(ERRoom.RoomStatus.Available, s);
        }

        [TestMethod]
        public void ToString_ContainsRoomId()
        {
            var r = new ERRoom { RoomId = 99, RoomTypeName = "Test", AvailabilityStatus = ERRoom.RoomStatus.Available };
            var s = r.ToString();
            Assert.IsTrue(s.Contains("Room 99"));
        }
    }
}
