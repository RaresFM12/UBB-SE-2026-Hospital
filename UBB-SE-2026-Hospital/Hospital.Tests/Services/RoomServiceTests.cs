using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using System;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class RoomServiceTests
    {
        [TestMethod]
        public void UpdateAvailabilityStatus_InvalidTransition_Throws()
        {
            var room = new ERRoom { RoomId = 5, AvailabilityStatus = ERRoom.RoomStatus.Available };

            Assert.ThrowsException<InvalidOperationException>(() => room.UpdateAvailabilityStatus("Cleaning"));
        }

        [TestMethod]
        public void UpdateAvailabilityStatus_ValidTransition_ChangesStatus()
        {
            var room = new ERRoom { RoomId = 6, AvailabilityStatus = ERRoom.RoomStatus.Available };

            room.UpdateAvailabilityStatus(ERRoom.RoomStatus.Occupied);

            Assert.AreEqual(ERRoom.RoomStatus.Occupied, room.AvailabilityStatus);
        }
    }
}
