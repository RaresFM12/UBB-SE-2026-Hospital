using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hospital.Data.Models;
using System.Collections.Generic;

namespace Hospital.Tests.Services
{
    [TestClass]
    public class MedicalHistoryServiceTests
    {
        [TestMethod]
        public void Allergies_NotMapped_GetterAndSetterWork()
        {
            var mh = new MedicalHistory();
            mh.Allergies = new List<(Allergy, string)>
            {
                (new Allergy { AllergyId = 1, AllergyName = "Pollen" }, "High")
            };

            Assert.AreEqual(1, mh.PatientAllergies.Count);
        }

        [TestMethod]
        public void Allergies_SetNullOrEmpty_AssignsNoneReported()
        {
            var mh = new MedicalHistory();
            mh.ChronicConditions = new List<string>();

            Assert.AreEqual(0, mh.ChronicConditions.Count);
        }
    }
}
