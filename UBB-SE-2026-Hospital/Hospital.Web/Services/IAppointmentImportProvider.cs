using Hospital.Data.Models.DTOs;

namespace Hospital.Web.Services;

public interface IAppointmentImportProvider
{
    MedicalRecordDetails FetchRecordByPatientId(int patientId);
}
