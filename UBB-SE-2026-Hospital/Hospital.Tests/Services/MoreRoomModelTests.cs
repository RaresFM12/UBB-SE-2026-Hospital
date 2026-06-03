using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using System;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class MoreRoomModelTests
    {
        [TestMethod]
        public void T124_ERRoom_StatusNormalize_UnknownKeeps()
        {
            var v = ERRoom.NormalizeStatus("strange");
            Assert.AreEqual("strange", v);
        }

        [TestMethod]
        public void T125_ERRoom_StatusEquals_CaseInsensitive()
        {
            Assert.IsTrue(ERRoom.StatusEquals("Available", "available"));
        }

        [TestMethod]
        public void T126_ERRoom_ToString_IncludesTypeAndId()
        {
            var r = new ERRoom { RoomId = 12, RoomTypeName = "ICU", AvailabilityStatus = ERRoom.RoomStatus.Available };
            var s = r.ToString();
            Assert.IsTrue(s.Contains("Room") && s.Contains("ICU"));
        }

        [TestMethod]
        public void T127_ERRoom_UpdateAvailability_FullCycle()
        {
            var r = new ERRoom { RoomId = 13, AvailabilityStatus = ERRoom.RoomStatus.Available };
            r.UpdateAvailabilityStatus(ERRoom.RoomStatus.Occupied);
            r.UpdateAvailabilityStatus(ERRoom.RoomStatus.Cleaning);
            r.UpdateAvailabilityStatus(ERRoom.RoomStatus.Available);
            Assert.AreEqual(ERRoom.RoomStatus.Available, r.AvailabilityStatus);
        }
    }
}
