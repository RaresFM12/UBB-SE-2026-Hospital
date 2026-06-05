using Hospital.Data.Models;

namespace Hospital.Web.Services;

public interface IAppointmentImportProvider
{
    MedicalRecordDetails FetchRecordByPatientId(int patientId);
}
