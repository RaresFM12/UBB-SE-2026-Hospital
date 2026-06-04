using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Services.PatientEr;
using System.Threading.Tasks;
using System.Collections.Generic;
using Hospital.Data.Models;
using Hospital.Data.Repositories;
using Moq;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class PatientServiceTests2
    {
        [TestMethod]
        public async Task GetPatientsAsync_WhenEmpty_ReturnsEmpty()
        {
            var repo = new Mock<IPatientRepository>();
            repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Patient>());
            var medicalHistory = new Mock<IMedicalHistoryRepository>();
            var medicalRecord = new Mock<IMedicalRecordRepository>();
            var prescription = new Mock<IPrescriptionRepository>();
            var svc = new PatientService(repo.Object, medicalHistory.Object, medicalRecord.Object, prescription.Object);
            var res = await svc.GetPatientsAsync();
            Assert.AreEqual(0, res.Count);
        }

        [TestMethod]
        public async Task GetPatientsAsync_WhenHasItems_ReturnsMapped()
        {
            var repo = new Mock<IPatientRepository>();
            repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Patient> { new Patient { PatientId = 1, FirstName = "Ana", LastName = "Pop" } });
            var medicalHistory = new Mock<IMedicalHistoryRepository>();
            var medicalRecord = new Mock<IMedicalRecordRepository>();
            var prescription = new Mock<IPrescriptionRepository>();
            var svc = new PatientService(repo.Object, medicalHistory.Object, medicalRecord.Object, prescription.Object);
            var res = await svc.GetPatientsAsync();
            Assert.AreEqual(1, res.Count);
        }
    }
}
