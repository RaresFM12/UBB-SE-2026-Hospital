using Hospital.Data.Models.DTOs;
using Hospital.Data.Models;

namespace Hospital.Web.Services;

public class MockAppointmentImportProvider : IAppointmentImportProvider
{
    public MedicalRecordDetails FetchRecordByPatientId(int patientId)
    {
        return new MedicalRecordDetails
        {
            ExternalRecordId = patientId,
            Symptoms = "Persistent headache",
            TemporaryDiagnosis = "Migraine",
            PrescribedMedications = "Sumatriptan 50mg",
            ConsultationDate = DateTime.Now,
            SourceType = SourceType.App,
        };
    }
}
