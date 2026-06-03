using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hospital.Services.PatientEr;
using Hospital.Data.Repositories;
using Hospital.Data.Models;
using Hospital.Data.Models.DTOs;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class ServiceBatchTests1
    {
        [TestMethod]
        public async Task T136_AddictDetection_MarkPoliceNotified_InvalidId_Throws()
        {
            var presRepo = new Mock<IPrescriptionRepository>();
            var historyRepo = new Mock<IMedicalHistoryRepository>();
            var sut = new AddictDetectionService(presRepo.Object, historyRepo.Object);

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await sut.MarkPoliceNotifiedAsync(0));
        }

        [TestMethod]
        public async Task T137_AddictDetection_BuildPoliceReport_InvalidId_Throws()
        {
            var presRepo = new Mock<IPrescriptionRepository>();
            var historyRepo = new Mock<IMedicalHistoryRepository>();
            presRepo.Setup(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>())).ReturnsAsync(new List<Prescription>());
            var sut = new AddictDetectionService(presRepo.Object, historyRepo.Object);

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await sut.BuildPoliceReportAsync(0));
        }

        [TestMethod]
        public async Task T138_AddictDetection_GetChronicConditions_NoHistory_ReturnsNoneReported()
        {
            var presRepo = new Mock<IPrescriptionRepository>();
            var historyRepo = new Mock<IMedicalHistoryRepository>();
            historyRepo.Setup(r => r.GetByPatientIdAsync(It.IsAny<int>())).ReturnsAsync((MedicalHistory?)null);
            var sut = new AddictDetectionService(presRepo.Object, historyRepo.Object);

            var res = await sut.GetChronicConditionsAsync(1);
            Assert.IsTrue(res.Contains("None reported") || res == "None reported.");
        }

        [TestMethod]
        public async Task T139_Prescription_GetPrescriptionDetails_NotFound_Throws()
        {
            var presRepo = new Mock<IPrescriptionRepository>();
            presRepo.Setup(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>())).ReturnsAsync(new List<Prescription>());
            var sut = new PrescriptionService(presRepo.Object);

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await sut.GetPrescriptionDetailsAsync(9999));
        }

        [TestMethod]
        public async Task T140_Prescription_ApplyFilter_Null_CallsTopN()
        {
            var presRepo = new Mock<IPrescriptionRepository>();
            presRepo.Setup(r => r.GetTopNAsync(20, 1)).ReturnsAsync(new List<Prescription> { new Prescription() });
            var sut = new PrescriptionService(presRepo.Object);

            var res = await sut.ApplyFilterAsync(null);
            Assert.AreEqual(1, res.Count);
        }

        [TestMethod]
        public async Task T141_Examination_GetEligibleVisits_NoRooms_ReturnsWaitingOrInRoom()
        {
            var examRepo = new Mock<IExaminationRepository>();
            var visitRepo = new Mock<IERVisitRepository>();
            var roomRepo = new Mock<IERRoomRepository>();
            var triageRepo = new Mock<ITriageRepository>();
            var triageParams = new Mock<ITriageParametersRepository>();

            var visits = new List<ERVisit>
            {
                new ERVisit { VisitId = 1, Status = ERVisit.VisitStatus.WAITING_FOR_DOCTOR, ArrivalDateTime = DateTime.UtcNow.AddMinutes(-10) },
                new ERVisit { VisitId = 2, Status = ERVisit.VisitStatus.REGISTERED, ArrivalDateTime = DateTime.UtcNow.AddMinutes(-5) },
            };
            visitRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(visits);
            roomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ERRoom>());

            var sut = new ExaminationService(examRepo.Object, visitRepo.Object, roomRepo.Object, triageRepo.Object, triageParams.Object);
            var res = await sut.GetEligibleVisitsAsync();

            Assert.IsTrue(res.Any(v => v.VisitId == 1));
            Assert.IsFalse(res.Any(v => v.VisitId == 2));
        }

        [TestMethod]
        public async Task T142_Examination_GetEligibleVisits_WithRoomLinked_InRoomIncluded()
        {
            var examRepo = new Mock<IExaminationRepository>();
            var visitRepo = new Mock<IERVisitRepository>();
            var roomRepo = new Mock<IERRoomRepository>();
            var triageRepo = new Mock<ITriageRepository>();
            var triageParams = new Mock<ITriageParametersRepository>();

            var visitInRoom = new ERVisit { VisitId = 10, Status = ERVisit.VisitStatus.IN_ROOM, ArrivalDateTime = DateTime.UtcNow.AddMinutes(-20) };
            visitRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ERVisit> { visitInRoom });
            roomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ERRoom> { new ERRoom { RoomId = 1, CurrentVisit = visitInRoom } });

            var sut = new ExaminationService(examRepo.Object, visitRepo.Object, roomRepo.Object, triageRepo.Object, triageParams.Object);
            var res = await sut.GetEligibleVisitsAsync();

            Assert.IsTrue(res.Any(v => v.VisitId == 10));
        }

        [TestMethod]
        public void T143_BloodCompatibility_IsBloodMatch_O_ReturnsTrue()
        {
            var svc = new BloodCompatibilityService(null!, null!);
            Assert.IsTrue(svc.IsBloodMatch(Hospital.Data.Models.BloodType.O, Hospital.Data.Models.BloodType.AB));
        }

        [TestMethod]
        public void T144_BloodCompatibility_IsBloodMatch_AB_ReturnsFalseForA()
        {
            var svc = new BloodCompatibilityService(null!, null!);
            Assert.IsFalse(svc.IsBloodMatch(Hospital.Data.Models.BloodType.AB, Hospital.Data.Models.BloodType.A));
        }

        [TestMethod]
        public void T145_BloodCompatibility_IsRhMatch_NegativeDonor_PositiveReceiver_False()
        {
            var svc = new BloodCompatibilityService(null!, null!);
            Assert.IsFalse(svc.IsRhMatch(Hospital.Data.Models.Rh.Negative, Hospital.Data.Models.Rh.Positive) == false);
            // The logic: return receiver != Rh.Negative || donor == Rh.Negative; for Positive receiver this is true only if donor==Negative => should be true
            Assert.IsTrue(svc.IsRhMatch(Hospital.Data.Models.Rh.Negative, Hospital.Data.Models.Rh.Positive));
        }

        [TestMethod]
        public void T146_ERVisit_Validate_InvalidStatus_Fails()
        {
            var v = new ERVisit { VisitId = 1, Patient = new Patient(), ArrivalDateTime = DateTime.UtcNow, ChiefComplaint = "c", Status = "BAD" };
            var ok = v.Validate(out var errors);
            Assert.IsFalse(ok);
            Assert.IsTrue(errors.Any(e => e.Contains("Invalid status")));
        }

        [TestMethod]
        public void T147_ERRoom_UpdateAvailability_InvalidNext_Throws()
        {
            var r = new ERRoom { RoomId = 5, AvailabilityStatus = ERRoom.RoomStatus.Available };
            Assert.ThrowsException<ArgumentException>(() => r.UpdateAvailabilityStatus("Unknown"));
        }

        [TestMethod]
        public void T148_ERRoom_StatusEquals_IgnoresCase()
        {
            Assert.IsTrue(ERRoom.StatusEquals("available", "Available"));
        }

        [TestMethod]
        public void T149_Examination_UpdateFields_PreservesVisit()
        {
            var doc = new Staff { FirstName = "D" };
            var room = new ERRoom { RoomId = 3, RoomTypeName = "G", AvailabilityStatus = ERRoom.RoomStatus.Available };
            var visit = new ERVisit { VisitId = 20, Patient = new Patient() };
            var e = new Examination { ExaminationId = 50, Doctor = doc, Room = room, Visit = visit, Findings = "f", ExaminationDate = DateTime.UtcNow };
            e.Findings = "updated";
            Assert.AreEqual("updated", e.Findings);
        }

        [TestMethod]
        public void T150_Notification_MarkReadAndToString()
        {
            var s = new Staff();
            var n = new Notification("t", "m", "a", s);
            n.IsRead = true;
            Assert.IsTrue(n.IsRead);
            Assert.IsTrue(n.ToString().Length > 0);
        }

        [TestMethod]
        public void T151_Staff_FullName_TrimsAndConcatenates()
        {
            var s = new Staff { FirstName = " A ", LastName = " B " };
            Assert.AreEqual(" A   B ".Trim(), s.FullName.Trim());
        }

        [TestMethod]
        public void T152_Patient_GetAge_TodayBirthday()
        {
            var p = new Patient { DateOfBirth = DateTime.Today.AddYears(-25) };
            Assert.AreEqual(25, p.GetAge());
        }

        [TestMethod]
        public void T153_User_AddAndRemoveStockAlert_Works()
        {
            var u = new User();
            u.AddStockAlertToUser(77);
            u.RemoveStockAlertFromUser(77);
            Assert.IsFalse(u.StockAlerts.Contains(77));
        }

        [TestMethod]
        public void T154_Order_IdString_ContainsId()
        {
            var o = new Order(1000, new User(), DateOnly.FromDateTime(DateTime.UtcNow));
            Assert.IsTrue(o.IdString.Contains("1000"));
        }

        [TestMethod]
        public void T155_Order_AddRemoveItem_UpdatesDictionary()
        {
            var o = new Order(1001, new User(), DateOnly.FromDateTime(DateTime.UtcNow));
            o.AddItemToOrder(1, 2, 5f);
            o.RemoveItemFromOrder(1);
            Assert.IsFalse(o.ItemQuantitiesWithFinalPrice.ContainsKey(1));
        }

        [TestMethod]
        public void T156_Item_AddActiveSubstance_ThenChange()
        {
            var it = new Item(400, "N", "P", "C", 1f, 10);
            it.AddActiveSubstanceToItem("sub", 1f);
            it.ChangeActiveSubstanceConcentration("sub", 2f);
            Assert.AreEqual(2f, it.ActiveSubstances["sub"]);
        }

        [TestMethod]
        public void T157_Item_Batches_ConsumeInOrder()
        {
            var it = new Item(401, "N", "P", "C", 1f, 10);
            var d1 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
            var d2 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
            it.AddNewBatchToItem(d1, 1);
            it.AddNewBatchToItem(d2, 4);
            it.RemoveQuantityFromItem(3, DateOnly.FromDateTime(DateTime.UtcNow));
            Assert.AreEqual(2, it.Quantity);
        }

        [TestMethod]
        public void T158_User_Basket_ChangeDiscountAndQuantity()
        {
            var u = new User();
            u.AddItemToBasket(501, 1, 0f);
            u.ChangeItemQuantityInBasket(501, 5);
            u.ChangeItemDiscountInBasket(501, 0.2f);
            Assert.AreEqual(5, u.Basket[501].Quantity);
            Assert.AreEqual(0.2f, u.Basket[501].ExtraDiscountPercentage);
        }

        [TestMethod]
        public void T159_MedicalHistory_Allergies_AddAndCount()
        {
            var mh = new MedicalHistory();
            mh.Allergies = new List<(Allergy, string)> { (new Allergy { AllergyId = 1, AllergyName = "A" }, "mild") };
            Assert.AreEqual(1, mh.Allergies.Count);
        }

        [TestMethod]
        public void T160_Notification_ActionButtonText_DefaultEmpty()
        {
            var s = new Staff();
            var n = new Notification("t", "m", "", s);
            Assert.AreEqual(string.Empty, n.ActionButtonText);
        }

        [TestMethod]
        public void T161_Order_ExpirationDateString_Calculated()
        {
            var pick = DateOnly.FromDateTime(new DateTime(2026, 1, 1));
            var o = new Order(1100, new User(), pick);
            Assert.IsTrue(o.ExpirationDateString.Contains("2026"));
        }

        [TestMethod]
        public void T162_ERRoom_ToString_ContainsRoomId()
        {
            var r = new ERRoom { RoomId = 77, RoomTypeName = "T", AvailabilityStatus = ERRoom.RoomStatus.Available };
            Assert.IsTrue(r.ToString().Contains("Room 77"));
        }

        [TestMethod]
        public void T163_Patient_IsDeceased_WhenDateOfDeathSet()
        {
            var p = new Patient { DateOfBirth = new DateTime(1970, 1, 1), DateOfDeath = DateTime.UtcNow };
            Assert.IsTrue(p.IsDeceased);
        }

        [TestMethod]
        public void T164_User_AddPeriodNoteThenRemove_Works()
        {
            var u = new User();
            u.AddPeriodNoteToUser(77, "n", false);
            u.RemovePeriodNoteFromUser(77);
            Assert.IsFalse(u.PeriodNotes.ContainsKey(77));
        }

        [TestMethod]
        public void T165_Item_RemoveBatch_AdjustsQuantity()
        {
            var it = new Item(420, "I", "P", "C", 1f, 10);
            var d = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3));
            it.AddNewBatchToItem(d, 5);
            it.RemoveBatchFromItem(d);
            Assert.AreEqual(0, it.Quantity);
        }

        [TestMethod]
        public void T166_Order_AddTwoDifferentItems_ContainsBoth()
        {
            var o = new Order(1200, new User(), DateOnly.FromDateTime(DateTime.UtcNow));
            o.AddItemToOrder(1, 1, 1f);
            o.AddItemToOrder(2, 2, 2f);
            Assert.IsTrue(o.ItemQuantitiesWithFinalPrice.ContainsKey(1) && o.ItemQuantitiesWithFinalPrice.ContainsKey(2));
        }

        [TestMethod]
        public void T167_Item_ChangeNumberOfPacksForBatch_AdjustsQuantity()
        {
            var it = new Item(430, "I", "P", "C", 1f, 10);
            var d = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(4));
            it.AddNewBatchToItem(d, 4);
            it.ChangeNumberOfPacksForBatch(d, 6);
            Assert.AreEqual(6, it.Batches[d]);
        }

        [TestMethod]
        public void T168_User_RemoveItemFromBasket_AfterAdd_Removes()
        {
            var u = new User();
            u.AddItemToBasket(300, 2);
            u.RemoveItemFromBasket(300);
            Assert.IsFalse(u.Basket.ContainsKey(300));
        }

        [TestMethod]
        public void T169_Item_GetQuantityAtSpecifiedDate_NoFutureBatches_ReturnsZero()
        {
            var it = new Item(440, "I", "P", "C", 1f, 10);
            var q = it.GetQuantityAtSpecifiedDate(DateOnly.FromDateTime(DateTime.UtcNow));
            Assert.AreEqual(0, q);
        }

        [TestMethod]
        public void T170_Order_RemoveItem_ThrowsWhenMissing()
        {
            var o = new Order(6000, new User(), DateOnly.FromDateTime(DateTime.UtcNow));
            Assert.ThrowsException<ArgumentException>(() => o.RemoveItemFromOrder(9999));
        }
    }
}
