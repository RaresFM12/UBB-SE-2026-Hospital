using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Hospital.Data.Models;

public class MedicalRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RecordId { get; set; }

    [Required]
    public int MedicalHistoryId { get; set; }

    [Required]
    [JsonIgnore]
    public MedicalHistory MedicalHistory { get; set; } = null!;

    public SourceType SourceType { get; set; }
    public int SourceId { get; set; }
    public int StaffId { get; set; }

    [MaxLength(200)]
    public string? Symptoms { get; set; }

    [MaxLength(100)]
    public string? Diagnosis { get; set; }

    [Required]
    public DateTime ConsultationDate { get; set; }

    public Prescription? Prescription { get; set; }

    [Required]
    public decimal BasePrice { get; set; }

    [Required]
    public decimal FinalPrice { get; set; }

    public int? DiscountApplied { get; set; }

    [Required]
    public bool PoliceNotified { get; set; }

    public int? TransplantId { get; set; }
}
