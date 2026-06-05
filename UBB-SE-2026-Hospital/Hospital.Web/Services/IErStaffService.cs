using Hospital.Data.Models;

namespace Hospital.Web.Services;

public interface IErStaffService
{
    int? RequestAvailableNurse();
    ErDoctorAssignment RequestDoctor(string specialization, TriageParameters parameters);
    ErDoctorAssignment GetDoctorById(int doctorId);
}

public sealed record ErDoctorAssignment(int doctorId, string name, string specialty);
