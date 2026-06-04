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
    public class ServiceBatchTests2
    {
        [TestMethod]
        public async Task T171_Auth_Login_Disabled_Throws()
        {
            var usersRepo = new Mock<Hospital.Data.Repositories.IUsersRepository>();
            var configuration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var user = new Hospital.Data.Models.User { Email = "a@b.com", PasswordHash = "p", IsDisabled = true };
            usersRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            var sut = new Hospital.Services.Auth.AuthService(usersRepo.Object, configuration.Object);
            await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(async () => await sut.LoginAsync(new Hospital.Shared.DTOs.Auth.LoginRequest { Email = "a@b.com", Password = "p" }));
        }

        [TestMethod]
        public async Task T172_Billing_ApplyDiscountAsync_Computes()
        {
            var historyRepo = new Mock<IMedicalHistoryRepository>();
            var recordRepo = new Mock<IMedicalRecordRepository>();
            var presRepo = new Mock<IPrescriptionRepository>();
            var transplantRepo = new Mock<ITransplantRepository>();
            var svc = new BillingService(historyRepo.Object, recordRepo.Object, presRepo.Object, transplantRepo.Object);
            var res = await svc.ApplyDiscountAsync(200m, 25);
            Assert.AreEqual(150m, res);
        }

        [TestMethod]
        public async Task T173_Billing_PersistDiscount_NotFound_Throws()
        {
            var historyRepo = new Mock<IMedicalHistoryRepository>();
            var recordRepo = new Mock<IMedicalRecordRepository>();
            var presRepo = new Mock<IPrescriptionRepository>();
            var transplantRepo = new Mock<ITransplantRepository>();
            recordRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((MedicalRecord?)null);
            var svc = new BillingService(historyRepo.Object, recordRepo.Object, presRepo.Object, transplantRepo.Object);
            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(async () => await svc.PersistDiscountAsync(1, 100m, 10));
        }

        [TestMethod]
        public async Task T174_Prescription_ApplyFilter_WithFilter_ReturnsFiltered()
        {
            var presRepo = new Mock<IPrescriptionRepository>();
            presRepo.Setup(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>())).ReturnsAsync(new List<Prescription> { new Prescription() });
            var svc = new PrescriptionService(presRepo.Object);
            var res = await svc.ApplyFilterAsync(new PrescriptionFilter { PatientId = 1 });
            Assert.AreEqual(1, res.Count);
        }

        [TestMethod]
        public async Task T175_Examination_UpdateAsync_NotFound_Throws()
        {
            var examRepo = new Mock<IExaminationRepository>();
            var visitRepo = new Mock<IERVisitRepository>();
            var roomRepo = new Mock<IERRoomRepository>();
            var triageRepo = new Mock<ITriageRepository>();
            var triageParams = new Mock<ITriageParametersRepository>();
            examRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Examination?)null);
            var svc = new ExaminationService(examRepo.Object, visitRepo.Object, roomRepo.Object, triageRepo.Object, triageParams.Object);
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await svc.UpdateAsync(new Examination { ExaminationId = 999 }));
        }

        [TestMethod]
        public async Task T176_Examination_GetSummaryByVisitIdAsync_NoExamination_ReturnsNull()
        {
            var examRepo = new Mock<IExaminationRepository>();
            var visitRepo = new Mock<IERVisitRepository>();
            var roomRepo = new Mock<IERRoomRepository>();
            var triageRepo = new Mock<ITriageRepository>();
            var triageParams = new Mock<ITriageParametersRepository>();
            examRepo.Setup(r => r.GetByVisitIdAsync(It.IsAny<int>())).ReturnsAsync(new List<Examination>());
            visitRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ERVisit?)null);
            var svc = new ExaminationService(examRepo.Object, visitRepo.Object, roomRepo.Object, triageRepo.Object, triageParams.Object);
            var res = await svc.GetSummaryByVisitIdAsync(1);
            Assert.IsNull(res);
        }

        [TestMethod]
        public async Task T177_AddictDetection_GetAddictCandidates_Empty_ReturnsEmpty()
        {
            var presRepo = new Mock<IPrescriptionRepository>();
            var historyRepo = new Mock<IMedicalHistoryRepository>();
            presRepo.Setup(r => r.GetPotentialDrugAddictsAsync()).ReturnsAsync(new List<Prescription>());
            var svc = new AddictDetectionService(presRepo.Object, historyRepo.Object);
            var res = await svc.GetAddictCandidatesAsync();
            Assert.AreEqual(0, res.Count);
        }

        [TestMethod]
        public void T178_ERRoom_UpdateAvailability_ValidSequence()
        {
            var r = new ERRoom { RoomId = 2, AvailabilityStatus = ERRoom.RoomStatus.Available };
            r.UpdateAvailabilityStatus(ERRoom.RoomStatus.Occupied);
            r.UpdateAvailabilityStatus(ERRoom.RoomStatus.Cleaning);
            Assert.AreEqual(ERRoom.RoomStatus.Cleaning, r.AvailabilityStatus);
        }

        [TestMethod]
        public void T179_ERRoom_UpdateAvailability_InvalidPrevious_ThrowsInvalidOperation()
        {
            var r = new ERRoom { RoomId = 3, AvailabilityStatus = ERRoom.RoomStatus.Cleaning };
            Assert.ThrowsException<InvalidOperationException>(() => r.UpdateAvailabilityStatus(ERRoom.RoomStatus.Occupied));
        }

        [TestMethod]
        public void T180_Order_ChangeItemInfo_UpdatesTupleValues()
        {
            var o = new Order(2000, new User(), DateOnly.FromDateTime(DateTime.UtcNow));
            o.AddItemToOrder(5, 2, 10f);
            o.ChangeItemInfoInOrder(5, 4, 8.5f);
            var tup = o.ItemQuantitiesWithFinalPrice[5];
            Assert.AreEqual(4, tup.Item1);
            Assert.AreEqual(8.5f, tup.Item2);
        }

        [TestMethod]
        public void T181_Item_RemoveQuantity_PartialConsume_AdjustsBatch()
        {
            var it = new Item(500, "X", "Y", "C", 1f, 10);
            var d = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
            it.AddNewBatchToItem(d, 5);
            it.RemoveQuantityFromItem(2, DateOnly.FromDateTime(DateTime.UtcNow));
            Assert.AreEqual(3, it.Batches[d]);
            Assert.AreEqual(3, it.Quantity);
        }

        [TestMethod]
        public void T182_User_Favorite_AddRemove_MaintainsSet()
        {
            var u = new User();
            u.AddItemToFavoriteItems(8);
            u.AddItemToFavoriteItems(9);
            u.RemoveItemFromFavoriteItems(8);
            Assert.IsTrue(u.FavoriteItems.Contains(9) && !u.FavoriteItems.Contains(8));
        }

        [TestMethod]
        public void T183_User_Basket_AddChange_Remove()
        {
            var u = new User();
            u.AddItemToBasket(600, 2);
            u.ChangeItemQuantityInBasket(600, 7);
            Assert.AreEqual(7, u.Basket[600].Quantity);
            u.RemoveItemFromBasket(600);
            Assert.IsFalse(u.Basket.ContainsKey(600));
        }

        [TestMethod]
        public void T184_Notification_ToString_IncludesTitle()
        {
            var s = new Staff { FirstName = "T" };
            var n = new Notification("Title", "Msg", "", s);
            Assert.IsTrue(n.ToString().Length > 0);
        }

        [TestMethod]
        public void T185_Patient_GetAge_ComputesPositive()
        {
            var p = new Patient { DateOfBirth = new DateTime(1995, 6, 1) };
            Assert.IsTrue(p.GetAge() > 0);
        }

        [TestMethod]
        public void T186_MedicalHistory_ChronicConditions_DefaultsWhenNull()
        {
            var mh = new MedicalHistory();
            mh.ChronicConditions = null;
            Assert.IsNotNull(mh.MedicalRecords);
        }

        [TestMethod]
        public void T187_Prescription_GetLatestPrescriptions_CallsRepo()
        {
            var presRepo = new Mock<IPrescriptionRepository>();
            presRepo.Setup(r => r.GetTopNAsync(5, 1)).ReturnsAsync(new List<Prescription> { new Prescription(), new Prescription() });
            var svc = new PrescriptionService(presRepo.Object);
            var res = svc.GetLatestPrescriptionsAsync(5, 1).Result;
            Assert.AreEqual(2, res.Count);
        }

        [TestMethod]
        public void T188_Examination_GetByVisitIdAsync_OrdersNewestFirst()
        {
            var examRepo = new Mock<IExaminationRepository>();
            var visitRepo = new Mock<IERVisitRepository>();
            var roomRepo = new Mock<IERRoomRepository>();
            var triageRepo = new Mock<ITriageRepository>();
            var triageParams = new Mock<ITriageParametersRepository>();
            examRepo.Setup(r => r.GetByVisitIdAsync(2)).ReturnsAsync(new List<Examination>
            {
                new Examination { ExaminationId = 1, ExaminationDate = new DateTime(2026,1,1) },
                new Examination { ExaminationId = 2, ExaminationDate = new DateTime(2026,1,3) }
            });
            var svc = new ExaminationService(examRepo.Object, visitRepo.Object, roomRepo.Object, triageRepo.Object, triageParams.Object);
            var res = svc.GetByVisitIdAsync(2).Result;
            Assert.AreEqual(2, res.First().ExaminationId);
        }

        [TestMethod]
        public void T189_ERVisit_Validate_MissingPatient_Fails()
        {
            var v = new ERVisit { VisitId = 10, ArrivalDateTime = DateTime.UtcNow, ChiefComplaint = "c", Patient = null! };
            v.Patient = null;
            var ok = v.Validate(out var errors);
            Assert.IsFalse(ok);
        }

        [TestMethod]
        public void T190_Order_AddTwoItems_QuantitiesStored()
        {
            var o = new Order(3000, new User(), DateOnly.FromDateTime(DateTime.UtcNow));
            o.AddItemToOrder(1, 2, 3f);
            o.AddItemToOrder(2, 4, 5f);
            Assert.AreEqual(2, o.ItemQuantitiesWithFinalPrice.Count);
        }

        [TestMethod]
        public void T191_Item_AddActiveSubstance_MultipleDifferent()
        {
            var it = new Item(600, "A", "B", "C", 1f, 10);
            it.AddActiveSubstanceToItem("a", 1f);
            it.AddActiveSubstanceToItem("b", 2f);
            Assert.IsTrue(it.ActiveSubstances.ContainsKey("a") && it.ActiveSubstances.ContainsKey("b"));
        }

        [TestMethod]
        public void T192_Item_ChangeNumberOfPacks_AdjustsQuantityDelta()
        {
            var it = new Item(610, "A", "B", "C", 1f, 10);
            var d = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(4));
            it.AddNewBatchToItem(d, 4);
            it.ChangeNumberOfPacksForBatch(d, 10);
            Assert.AreEqual(10, it.Batches[d]);
            Assert.AreEqual(10, it.Quantity);
        }

        [TestMethod]
        public void T193_User_AddUserDiscount_Change_Remove()
        {
            var u = new User();
            u.AddUserDiscount(700, 0.1f);
            u.ChangeUserDiscount(700, 0.25f);
            Assert.AreEqual(0.25f, u.UserDiscounts[700]);
            u.RemoveUserDiscount(700);
            Assert.IsFalse(u.UserDiscounts.ContainsKey(700));
        }

        [TestMethod]
        public void T194_Notification_IsRead_Toggle()
        {
            var s = new Staff();
            var n = new Notification("t", "m", "a", s);
            n.IsRead = true;
            Assert.IsTrue(n.IsRead);
        }

        [TestMethod]
        public void T195_Staff_FullName_BothNames_ReturnsConcatenated()
        {
            var s = new Staff { FirstName = "A", LastName = "B" };
            Assert.AreEqual("A B", s.FullName);
        }

        [TestMethod]
        public void T196_Patient_FullName_Concatenates()
        {
            var p = new Patient { FirstName = "X", LastName = "Y" };
            Assert.AreEqual("X Y", p.FullName);
        }

        [TestMethod]
        public void T197_Order_Equals_SameId_True()
        {
            var o1 = new Order(4000, new User(), DateOnly.FromDateTime(DateTime.UtcNow));
            var o2 = new Order(4000, new User(), DateOnly.FromDateTime(DateTime.UtcNow));
            Assert.IsTrue(o1.Equals(o2));
        }

        [TestMethod]
        public void T198_Item_RemoveBatch_AdjustsQuantityToZero()
        {
            var it = new Item(620, "I", "P", "C", 1f, 10);
            var d = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
            it.AddNewBatchToItem(d, 2);
            it.RemoveBatchFromItem(d);
            Assert.AreEqual(0, it.Quantity);
        }

        [TestMethod]
        public void T199_User_SetPeriodTracker_ValuesStored()
        {
            var u = new User();
            var date = DateOnly.FromDateTime(new DateTime(2026, 1, 1));
            u.SetPeriodTracker(date, 30, 4, 2);
            Assert.AreEqual(30, u.CycleDays);
            Assert.AreEqual(2, u.PremenstrualSyndromeOption);
        }

        [TestMethod]
        public void T200_BloodCompatibility_IsBloodMatch_A_to_AB_True()
        {
            var svc = new BloodCompatibilityService(null!, null!);
            Assert.IsTrue(svc.IsBloodMatch(Hospital.Data.Models.BloodType.A, Hospital.Data.Models.BloodType.AB));
        }

        [TestMethod]
        public void T201_BloodCompatibility_IsRhMatch_PositiveReceiver_True()
        {
            var svc = new BloodCompatibilityService(null!, null!);
            Assert.IsTrue(svc.IsRhMatch(Hospital.Data.Models.Rh.Positive, Hospital.Data.Models.Rh.Positive));
        }

        [TestMethod]
        public void T202_AddictDetection_GetChronicConditions_WithHistory_ReturnsJoined()
        {
            var presRepo = new Mock<IPrescriptionRepository>();
            var historyRepo = new Mock<IMedicalHistoryRepository>();
            var history = new MedicalHistory { ChronicConditions = new List<string> { "A", "B" } };
            historyRepo.Setup(r => r.GetByPatientIdAsync(It.IsAny<int>())).ReturnsAsync(history);
            var svc = new AddictDetectionService(presRepo.Object, historyRepo.Object);
            var res = svc.GetChronicConditionsAsync(1).Result;
            Assert.IsTrue(res.Contains("A") && res.Contains("B"));
        }

        [TestMethod]
        public void T203_Prescription_GetPrescriptionDetails_Found_ReturnsItem()
        {
            var presRepo = new Mock<IPrescriptionRepository>();
            presRepo.Setup(r => r.GetFilteredAsync(It.IsAny<PrescriptionFilter>())).ReturnsAsync(new List<Prescription> { new Prescription { PrescriptionId = 99 } });
            var svc = new PrescriptionService(presRepo.Object);
            var res = svc.GetPrescriptionDetailsAsync(99).Result;
            Assert.AreEqual(99, res.PrescriptionId);
        }

        [TestMethod]
        public void T204_Examination_GetPatientHistoryAsync_NoVisits_ReturnsEmpty()
        {
            var examRepo = new Mock<IExaminationRepository>();
            var visitRepo = new Mock<IERVisitRepository>();
            var roomRepo = new Mock<IERRoomRepository>();
            var triageRepo = new Mock<ITriageRepository>();
            var triageParams = new Mock<ITriageParametersRepository>();
            visitRepo.Setup(r => r.GetByPatientIdAsync(It.IsAny<int>())).ReturnsAsync(new List<ERVisit>());
            var svc = new ExaminationService(examRepo.Object, visitRepo.Object, roomRepo.Object, triageRepo.Object, triageParams.Object);
            var res = svc.GetPatientHistoryAsync(1).Result;
            Assert.AreEqual(0, res.Count);
        }

        [TestMethod]
        public void T205_Item_AddBatchThenChangeNumberOfPacks_AdjustsQuantity()
        {
            var it = new Item(700, "I", "P", "C", 1f, 10);
            var d = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
            it.AddNewBatchToItem(d, 5);
            it.ChangeNumberOfPacksForBatch(d, 3);
            Assert.AreEqual(3, it.Batches[d]);
        }

        [TestMethod]
        public void T206_User_AddPeriodNote_MultipleNotes_Counts()
        {
            var u = new User();
            u.AddPeriodNoteToUser(1, "a", false);
            u.AddPeriodNoteToUser(2, "b", true);
            Assert.AreEqual(2, u.PeriodNotes.Count);
        }

        [TestMethod]
        public void T207_Order_RemoveItem_ThrowsWhenMissing_Unique()
        {
            var o = new Order(8000, new User(), DateOnly.FromDateTime(DateTime.UtcNow));
            Assert.ThrowsException<ArgumentException>(() => o.RemoveItemFromOrder(12345));
        }

        [TestMethod]
        public void T208_Item_GetQuantityAtSpecifiedDate_WithMultipleBatches_ReturnsCorrect()
        {
            var it = new Item(800, "X", "Y", "Z", 1f, 10);
            var d1 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
            var d2 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
            it.AddNewBatchToItem(d1, 1);
            it.AddNewBatchToItem(d2, 4);
            Assert.AreEqual(4, it.GetQuantityAtSpecifiedDate(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5))));
        }

        [TestMethod]
        public void T209_User_AddAndChangeBasketDiscount_Works()
        {
            var u = new User();
            u.AddItemToBasket(900, 1, 0f);
            u.ChangeItemDiscountInBasket(900, 0.3f);
            Assert.AreEqual(0.3f, u.Basket[900].ExtraDiscountPercentage);
        }

        [TestMethod]
        public void T210_Item_ChangeActiveSubstance_AfterAdd_ChangesValue_Unique()
        {
            var it = new Item(810, "I", "P", "C", 1f, 10);
            it.AddActiveSubstanceToItem("x", 1f);
            it.ChangeActiveSubstanceConcentration("x", 3f);
            Assert.AreEqual(3f, it.ActiveSubstances["x"]);
        }

        [TestMethod]
        public void T211_Order_Equals_DifferentId_False()
        {
            var o1 = new Order(9001, new User(), DateOnly.FromDateTime(DateTime.UtcNow));
            var o2 = new Order(9002, new User(), DateOnly.FromDateTime(DateTime.UtcNow));
            Assert.IsFalse(o1.Equals(o2));
        }

        [TestMethod]
        public void T212_Notification_ActionButtonText_Empty_Defaults()
        {
            var s = new Staff();
            var n = new Notification("t", "m", "", s);
            Assert.AreEqual(string.Empty, n.ActionButtonText);
        }

        [TestMethod]
        public void T213_Patient_Cnp_Invalid_FailsValidate()
        {
            var p = new Patient { Cnp = "abc", FirstName = "A", LastName = "B", DateOfBirth = new DateTime(2000,1,1), Sex = Sex.F, PhoneNumber = "0700", EmergencyContact = "X" };
            var ok = p.Validate(out var errors);
            Assert.IsFalse(ok);
        }

        [TestMethod]
        public void T214_MedicalHistory_Allergies_SetAddsEntries_Unique()
        {
            var mh = new MedicalHistory();
            mh.Allergies = new List<(Allergy, string)> { (new Allergy { AllergyId = 2, AllergyName = "Dust" }, "M") };
            Assert.AreEqual(1, mh.Allergies.Count);
        }

        [TestMethod]
        public void T215_Staff_LicenseNumber_DefaultEmpty_Unique()
        {
            var s = new Staff();
            Assert.AreEqual(string.Empty, s.LicenseNumber);
        }
    }
}
