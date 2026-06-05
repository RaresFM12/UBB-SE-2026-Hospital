using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Hospital.Data.Models;

public class Prescription
{
    [Key]
    public int PrescriptionId { get; set; }

    [Required]
    [JsonIgnore]
    public MedicalRecord MedicalRecord { get; set; } = null!;

    public List<PrescriptionItem> MedicationList { get; set; } = new();

    [MaxLength(2000)]
    public string? DoctorNotes { get; set; }

    [Required]
    public DateTime Date { get; set; }

    private string? patientName;

    [NotMapped]
    public string PatientName
    {
        get => patientName
            ?? (MedicalRecord?.MedicalHistory?.Patient?.FullName ?? string.Empty);
        set => patientName = value;
    }

    [NotMapped]
    public string DoctorName { get; set; } = "Unknown";
}
