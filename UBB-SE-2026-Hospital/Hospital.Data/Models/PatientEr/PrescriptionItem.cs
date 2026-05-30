using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Hospital.Data.Models;

public class PrescriptionItem
{
    [Key]
    public int PrescriptionItemId { get; set; }

    [JsonIgnore]
    public Prescription Prescription { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string MedicationName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Quantity { get; set; }
}
