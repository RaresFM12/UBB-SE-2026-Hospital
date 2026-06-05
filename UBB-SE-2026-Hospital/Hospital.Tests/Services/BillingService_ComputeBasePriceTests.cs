using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Services;
using Hospital.Data.Repositories;
using Hospital.Data.Models;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class BillingService_ComputeBasePriceTests
    {
        [TestMethod]
        public async Task ComputeBasePrice_RecordOrHistoryNull_ReturnsZero()
        {
            var mockHistoryRepo = new Mock<IMedicalHistoryRepository>();
            var mockRecordRepo = new Mock<IMedicalRecordRepository>();
            var mockPresRepo = new Mock<IPrescriptionRepository>();
            var mockTransRepo = new Mock<ITransplantRepository>();

            mockRecordRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((MedicalRecord?)null);
            mockHistoryRepo.Setup(r => r.GetByPatientIdAsync(It.IsAny<int>())).ReturnsAsync((MedicalHistory?)null);

            var service = new BillingService(mockHistoryRepo.Object, mockRecordRepo.Object, mockPresRepo.Object, mockTransRepo.Object);

            var result = await service.ComputeBasePriceAsync(1, 1);

            Assert.AreEqual(0m, result);
        }

        [TestMethod]
        public async Task ComputeBasePrice_ERWithPrescriptionsAndChronicConditions_ComputesExpected()
        {
            var mockHistoryRepo = new Mock<IMedicalHistoryRepository>();
            var mockRecordRepo = new Mock<IMedicalRecordRepository>();
            var mockPresRepo = new Mock<IPrescriptionRepository>();
            var mockTransRepo = new Mock<ITransplantRepository>();

            var record = new MedicalRecord { RecordId = 10, SourceType = SourceType.ER };
            mockRecordRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(record);

            var prescription = new Prescription { PrescriptionId = 1 };
            var prescriptionItems = new List<PrescriptionItem> { new PrescriptionItem(), new PrescriptionItem() };
            mockPresRepo.Setup(r => r.GetItemsAsync(It.IsAny<int>())).ReturnsAsync(prescriptionItems);

            var history = new MedicalHistory { ChronicConditions = new List<string> { "A" }, Allergies = new List<(Allergy, string)>() };
            mockHistoryRepo.Setup(r => r.GetByPatientIdAsync(It.IsAny<int>())).ReturnsAsync(history);

            mockTransRepo.Setup(r => r.GetByPatientIdAsync(It.IsAny<int>())).ReturnsAsync(new List<Transplant>());

            var service = new BillingService(mockHistoryRepo.Object, mockRecordRepo.Object, mockPresRepo.Object, mockTransRepo.Object);

            var computed = await service.ComputeBasePriceAsync(1, 10);

            // ER base 500 + 2*50 for prescription items + 1*100 chronic
            Assert.AreEqual(500m + 100m + 100m, computed);
        }

        [TestMethod]
        public async Task ComputeBasePrice_WithSevereAllergyAndTransplant_IncludesAdditions()
        {
            var mockHistoryRepo = new Mock<IMedicalHistoryRepository>();
            var mockRecordRepo = new Mock<IMedicalRecordRepository>();
            var mockPresRepo = new Mock<IPrescriptionRepository>();
            var mockTransRepo = new Mock<ITransplantRepository>();

            var record = new MedicalRecord { RecordId = 20, SourceType = SourceType.App };
            mockRecordRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(record);

            mockPresRepo.Setup(r => r.GetItemsAsync(It.IsAny<int>())).ReturnsAsync(new List<PrescriptionItem>());

            var allergy = new Allergy { AllergyId = 1, AllergyName = "Pollen" };
            var history = new MedicalHistory { ChronicConditions = new List<string>(), Allergies = new List<(Allergy, string)> { (allergy, "severe") } };
            mockHistoryRepo.Setup(r => r.GetByPatientIdAsync(It.IsAny<int>())).ReturnsAsync(history);

            mockTransRepo.Setup(r => r.GetByPatientIdAsync(It.IsAny<int>())).ReturnsAsync(new List<Transplant> { new Transplant() });

            var service = new BillingService(mockHistoryRepo.Object, mockRecordRepo.Object, mockPresRepo.Object, mockTransRepo.Object);

            var computed = await service.ComputeBasePriceAsync(2, 20);

            // Appointment base 200 + severe allergy 100 + transplant 2000
            Assert.AreEqual(200m + 100m + 2000m, computed);
        }
    }
}
