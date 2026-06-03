using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Hospital.Data.Repositories;
using Hospital.Data.Models;
using Hospital.Services.PatientEr;
using Moq;
using System.Collections.Generic;
using System;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class TransplantServiceTests2
    {
        [TestMethod]
        public async Task AssignDonorAsync_WhenTransplantMissing_Throws()
        {
            var tRepo = new Mock<ITransplantRepository>();
            var pRepo = new Mock<IPatientRepository>();
            var rRepo = new Mock<IMedicalRecordRepository>();
            var bc = new Mock<IBloodCompatibilityService>();
            var mh = new Mock<IMedicalHistoryRepository>();

            tRepo.Setup(t => t.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Transplant?)null);

            var svc = new TransplantService(tRepo.Object, pRepo.Object, rRepo.Object, bc.Object, mh.Object);

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await svc.AssignDonorAsync(1, 1, 0f));
        }

        [TestMethod]
        public async Task GetTopMatchesAsDisplayModelsAsync_DonorNotDeceased_Throws()
        {
            var tRepo = new Mock<ITransplantRepository>();
            var pRepo = new Mock<IPatientRepository>();
            var rRepo = new Mock<IMedicalRecordRepository>();
            var bc = new Mock<IBloodCompatibilityService>();
            var mh = new Mock<IMedicalHistoryRepository>();

            pRepo.Setup(p => p.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Patient { IsDonor = true });

            var svc = new TransplantService(tRepo.Object, pRepo.Object, rRepo.Object, bc.Object, mh.Object);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await svc.GetTopMatchesAsDisplayModelsAsync(1, "Kidney"));
        }
    }
}
