using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

namespace Hospital.Data.Models;

public class MedicalHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MedicalHistoryId { get; set; }

    [Required]
    [JsonIgnore]
    public Patient Patient { get; set; } = null!;

    public BloodType? BloodType { get; set; }
    public Rh? Rh { get; set; }

    public List<string> ChronicConditions { get; set; } = new();
    public List<MedicalRecord> MedicalRecords { get; set; } = new();
    public List<PatientAllergy> PatientAllergies { get; set; } = new();

    [NotMapped]
    public List<(Allergy Allergy, string SeverityLevel)> Allergies
    {
        get => PatientAllergies
            .Where(pa => pa?.Allergy is not null)
            .Select(pa => (pa.Allergy, pa.SeverityLevel))
            .ToList();
        set
        {
            PatientAllergies = value?
                .Where(item => item.Allergy is not null)
                .Select(item => new PatientAllergy
                {
                    Allergy = item.Allergy,
                    AllergyId = item.Allergy.AllergyId,
                    SeverityLevel = item.SeverityLevel ?? string.Empty,
                    MedicalHistoryId = MedicalHistoryId,
                })
                .ToList() ?? new List<PatientAllergy>();
        }
    }
}
