using System.Collections.Generic;

namespace Hospital.Data.Models;

public class Doctor : Staff
{
    public DoctorStatus DoctorStatus { get; set; } = DoctorStatus.AVAILABLE;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<MedicalEvaluation> MedicalEvaluations { get; set; } = new List<MedicalEvaluation>();

    public Doctor()
    {
        Role = "Doctor";
    }
}
