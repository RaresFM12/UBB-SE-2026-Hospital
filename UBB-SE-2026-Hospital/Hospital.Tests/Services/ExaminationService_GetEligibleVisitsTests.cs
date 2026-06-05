using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Services;
using Hospital.Data.Repositories;
using Hospital.Data.Models;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class ExaminationService_GetEligibleVisitsTests
    {
        [TestMethod]
        public async Task GetEligibleVisits_NoRooms_ReturnsFilteredVisits()
        {
            var mockExamRepo = new Mock<IExaminationRepository>();
            var mockVisitRepo = new Mock<IERVisitRepository>();
            var mockRoomRepo = new Mock<IERRoomRepository>();
            var mockTriageRepo = new Mock<ITriageRepository>();
            var mockParamsRepo = new Mock<ITriageParametersRepository>();

            mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ERRoom>());

            var visitA = new ERVisit { VisitId = 1, Status = ERVisit.VisitStatus.WAITING_FOR_DOCTOR, ArrivalDateTime = DateTime.UtcNow.AddHours(-2) };
            var visitB = new ERVisit { VisitId = 2, Status = ERVisit.VisitStatus.CLOSED, ArrivalDateTime = DateTime.UtcNow.AddHours(-1) };
            mockVisitRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ERVisit> { visitA, visitB });

            var service = new ExaminationService(mockExamRepo.Object, mockVisitRepo.Object, mockRoomRepo.Object, mockTriageRepo.Object, mockParamsRepo.Object);

            var resultVisits = await service.GetEligibleVisitsAsync();

            Assert.AreEqual(1, resultVisits.Count);
        }

        [TestMethod]
        public async Task GetEligibleVisits_RoomLinkedIncludesInRoomVisit()
        {
            var mockExamRepo = new Mock<IExaminationRepository>();
            var mockVisitRepo = new Mock<IERVisitRepository>();
            var mockRoomRepo = new Mock<IERRoomRepository>();
            var mockTriageRepo = new Mock<ITriageRepository>();
            var mockParamsRepo = new Mock<ITriageParametersRepository>();

            var visitInRoom = new ERVisit { VisitId = 3, Status = ERVisit.VisitStatus.IN_ROOM, ArrivalDateTime = DateTime.UtcNow.AddHours(-3) };
            var roomWithVisit = new ERRoom { RoomId = 1, CurrentVisit = visitInRoom };
            mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ERRoom> { roomWithVisit });

            mockVisitRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ERVisit> { visitInRoom });

            var service = new ExaminationService(mockExamRepo.Object, mockVisitRepo.Object, mockRoomRepo.Object, mockTriageRepo.Object, mockParamsRepo.Object);

            var result = await service.GetEligibleVisitsAsync();

            Assert.AreEqual(1, result.Count);
        }
    }
}
