using System.Collections.Generic;

namespace Hospital.Data.Models;

public class Doctor : Staff
{
    private const string DoctorRole = "Doctor";

    public DoctorStatus DoctorStatus { get; set; } = DoctorStatus.Available;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<MedicalEvaluation> MedicalEvaluations { get; set; } = new List<MedicalEvaluation>();

    public Doctor()
    {
        Role = DoctorRole;
    }
}
