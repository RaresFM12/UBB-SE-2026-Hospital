using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.Data.Models;

public class Allergy
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AllergyId { get; set; }

    [Required]
    [MaxLength(200)]
    public string AllergyName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? AllergyType { get; set; }

    [MaxLength(50)]
    public string? AllergyCategory { get; set; }
}
