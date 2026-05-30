using System;

namespace Hospital.Data.Models.DTOs;

public class PrescriptionFilter
{
    public int? PrescriptionId { get; set; }
    public int? PatientId { get; set; }
    public int? DoctorId { get; set; }
    public string? MedicationName { get; set; }
    public string? PatientName { get; set; }
    public string? DoctorName { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
