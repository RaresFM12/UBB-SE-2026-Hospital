using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hospital.Services.PatientEr;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Hospital.Tests.Services
{
    [TestClass]
    public sealed class TransplantServiceTests
    {
        private Mock<ITransplantRepository> transplantRepository = null!;
        private Mock<IPatientRepository> patientRepository = null!;
        private Mock<IMedicalRecordRepository> recordRepository = null!;
        private Mock<IBloodCompatibilityService> compatibilityService = null!;
        private Mock<IMedicalHistoryRepository> historyRepository = null!;
        private TransplantService sut = null!;

        [TestInitialize]
        public void Setup()
        {
            transplantRepository = new Mock<ITransplantRepository>();
            patientRepository = new Mock<IPatientRepository>();
            recordRepository = new Mock<IMedicalRecordRepository>();
            compatibilityService = new Mock<IBloodCompatibilityService>();
            historyRepository = new Mock<IMedicalHistoryRepository>();
            sut = new TransplantService(
                transplantRepository.Object,
                patientRepository.Object,
                recordRepository.Object,
                compatibilityService.Object,
                historyRepository.Object);
        }

        [TestMethod]
        public async Task CreateWaitlistRequestAsync_WhenReceiverDoesNotExist_ThrowsArgumentException()
        {
            patientRepository.Setup(x => x.GetByIdAsync(7)).ReturnsAsync((Patient?)null);

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await sut.CreateWaitlistRequestAsync(7, "Kidney"));
        }

        [TestMethod]
        public async Task IsUrgentAsync_WhenNoHistory_ReturnsFalse()
        {
            historyRepository.Setup(x => x.GetByPatientIdAsync(5)).ReturnsAsync((MedicalHistory?)null);

            var result = await sut.IsUrgentAsync(5);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetChronicWarningAsync_WhenHasConditions_ReturnsWarning()
        {
            historyRepository.Setup(x => x.GetByPatientIdAsync(9)).ReturnsAsync(new MedicalHistory { ChronicConditions = new List<string>{ "Diabetes" } });

            var result = await sut.GetChronicWarningAsync(9);

            Assert.IsNotNull(result);
        }
    }
}
