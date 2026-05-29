namespace Hospital.Shared.Models.PatientEr;

public class Prescription
{
    public int PrescriptionId { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public DateTime DateIssued { get; set; }

    public string Notes { get; set; } = string.Empty;
}
